using UnityEngine;

public interface IInteractable
{
    string InteractMessage { get; }
    void OnInteract(Vector3 interacterPosition);
}
