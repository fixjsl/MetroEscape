using UnityEngine;

public interface IInteractable
{
    bool isStuck { get; }
    string InteractMessage { get; }
    void OnInteract(Vector3 interacterPosition);
}
