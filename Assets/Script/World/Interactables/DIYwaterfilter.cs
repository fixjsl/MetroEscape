using System.Collections;
using UnityEngine;

class DIYwaterfilter : ItObjectBase
{
    public int fillteringTime;
    public bool isFilltering;
    private bool isFiltered;
    
    [SerializeField] private ItemBase OldWater;
    [SerializeField] private ItemBase Water;

    public override bool isStuck => false;
    public override string InteractMessage =>
        isFilltering ? "정수중" :
        isFiltered ? $"물 얻기 {InputBindings.Interact}" :
        $"물 넣기 {InputBindings.Interact}";

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        if (isFiltered)
        {
            if (PlayerStateMachine.Instance.inventory.AddItem(Water))
            {
                isFiltered = false;
                isInteracting = false;
                RefreshPrompt();
            }
            else
            {
                isInteracting = false;
            }
            return;
        }

        if (!PlayerStateMachine.Instance.inventory.RemoveItem(OldWater))
        {
            isInteracting = false;
            return;
        }

        isFilltering = true;
        RefreshPrompt();
        StartCoroutine(FilterRoutine());
    }

    private IEnumerator FilterRoutine()
    {
        yield return new WaitForSeconds(fillteringTime);
        isFilltering = false;
        isFiltered = true;
        isInteracting = false;
        RefreshPrompt();
    }
}
