using UnityEngine;

class CraftingTable : InteractableBase
{
    public override string InteractMessage => Prompt("제작대");

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        if (GameUI.Crafting != null) TogglePanel(GameUI.Crafting);
    }
    protected override void OnPlayerExited(PlayerStateMachine player)
    {
        if (GameUI.Crafting != null && GameUI.Crafting.IsOpen)
            GameUI.Crafting.Close();
    }
}
