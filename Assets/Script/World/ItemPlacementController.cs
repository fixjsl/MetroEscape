using UnityEngine;

public class ItemPlacementController : SingletonBehaviour<ItemPlacementController>
{

    [SerializeField] private Transform anchor;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private int maxCount = 5;

    private int count = 0;


    public bool TryPlace(PlaceableItem item)
    {
        if (count >= maxCount)
        {
            Debug.Log("[Placement] 최대 설치 개수 초과");
            return false;
        }

        Vector3 pos = anchor.position + Vector3.right * spacing * count;
        Instantiate(item.placePrefab, pos, anchor.rotation);
        count++;
        return true;
    }
}
