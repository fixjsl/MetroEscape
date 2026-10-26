using UnityEngine;

class CraftingTable : ItObjectBase
{
    public override string InteractMessage => Prompt("제작대");

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        TogglePanel(CraftingUI.Instance);
    }
    protected override void OnPlayerExited(PlayerStateMachine player)
    {
        if (CraftingUI.Instance.IsOpen)
            CraftingUI.Instance.Close();
    }
}
