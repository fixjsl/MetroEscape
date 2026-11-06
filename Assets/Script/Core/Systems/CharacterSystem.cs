using UnityEngine;

public abstract class CharacterSystem<TOwner> where TOwner : MonoBehaviour
{
    protected readonly TOwner owner;
    protected CharacterSystem(TOwner owner) => this.owner = owner;
    protected virtual float interval =>0f;
    public virtual void Start() { }
    public virtual void Tick() { }
    public virtual void Stop() { }
}
