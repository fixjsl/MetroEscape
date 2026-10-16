using UnityEngine;

[CreateAssetMenu(fileName = "PlaceableItem", menuName = "Scriptable Objects/Item/PlaceableItem")]
public class PlaceableItem : ItemBase
{
    public GameObject placePrefab;

    private void Reset() => itemType = ItemType.InteractObject;

    public override bool OnUse(PlayerStateMachine player)
    {
        return ItemPlacementController.Instance.TryPlace(this);
    }
}
