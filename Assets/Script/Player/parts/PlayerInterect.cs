using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract
{
    private readonly Transform owner;
    public PlayerInteract(Transform owner) => this.owner = owner;
    private List<IInteractable> nearby = new();
    public IInteractable Closest => GetClosest();
    public void SetInteractable(IInteractable interactable)
    {
        if (!nearby.Contains(interactable))
            nearby.Add(interactable);
    }
    public void ClearInteractable(IInteractable interactable)
    {
        nearby.Remove(interactable);
    }
    public IInteractable GetClosest()
    {
        IInteractable best = null;
        float bestDist = float.MaxValue;
        foreach(var i in nearby)
        {
            float d = (((MonoBehaviour)i).transform.position - owner.transform.position).sqrMagnitude;
            if(d<bestDist) { bestDist = d; best = i; }
        }
        return best;
    }
}
