using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private ItemBase[] items;
    public static Dictionary<int, ItemBase> itemDB { get; private set; }

    private void Awake()
    {
        itemDB = new Dictionary<int, ItemBase>();
        foreach (var item in items)
            itemDB[item.itemcode] = item;
    }
}
