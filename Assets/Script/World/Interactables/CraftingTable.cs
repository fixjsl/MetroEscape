using UnityEngine;

class CraftingTable : ItObjectBase
{
    public override bool isStuck => false;
    public override string InteractMessage => $"제작대 {InputBindings.Interact}";

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        if (CraftingUI.Instance.IsOpen)
            CraftingUI.Instance.Close();
        else
            CraftingUI.Instance.Open();
        isInteracting = false;
    }
        protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (other.TryGetComponent<PlayerStateMachine>(out _))
        {
            if (CraftingUI.Instance.IsOpen)
                CraftingUI.Instance.Close();
        }
    }
}
