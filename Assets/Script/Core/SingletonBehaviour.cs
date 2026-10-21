using UnityEngine;

public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    public static T Instance { get; protected set; }
    protected virtual bool Persistent => false;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this); return;
        }
        Instance = (T)this;
        if (Persistent)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
    }


    protected virtual void OnDestroy() { if (Instance == this) Instance = null; }
}
