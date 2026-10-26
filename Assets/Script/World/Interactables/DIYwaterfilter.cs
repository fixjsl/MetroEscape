using System.Collections;
using UnityEngine;

class DIYwaterfilter : ItObjectBase
{
    public int fillteringTime;
    public bool isFilltering;
    private bool isFiltered;
    
    [SerializeField] private ItemBase OldWater;
    [SerializeField] private ItemBase Water;

    public override string InteractMessage =>
        isFilltering ? "정수중" :
        Prompt(isFiltered ? "물 얻기" : "물 넣기");

    protected override void OnInteractInternal(Vector3 interacterPosition)
    {
        if (isFiltered)
        {
            if (PlayerStateMachine.Instance.inventory.AddItem(Water))
            {
                isFiltered = false;
                RefreshPrompt();
            }
            return;
        }

        if (!PlayerStateMachine.Instance.inventory.RemoveItem(OldWater))
        {
            return;
        }

        isFilltering = true;
        RefreshPrompt();
        BeginBusy();
        StartCoroutine(FilterRoutine());
    }

    private IEnumerator FilterRoutine()
    {
        yield return new WaitForSeconds(fillteringTime);
        isFilltering = false;
        isFiltered = true;
        EndBusy();
        RefreshPrompt();
    }
}
