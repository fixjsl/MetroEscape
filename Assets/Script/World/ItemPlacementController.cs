using UnityEngine;

public class ItemPlacementController : MonoBehaviour
{
    public static ItemPlacementController Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [SerializeField] private Transform anchor;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private int maxCount = 5;

    private int count = 0;

    void Awake() => Instance = this;

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
