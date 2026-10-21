using System.Collections.Generic;
using UnityEngine;

public class ItemManager : SingletonBehaviour<ItemManager>
{
    [SerializeField] private ItemDatabase database;

    private static ItemDatabase db;

    public static bool TryGet(int code, out ItemBase item) => db.TryGet(code, out item);
    public static IReadOnlyList<ItemBase> Items => db.Items;
    public static IReadOnlyList<RecipeSO> Recipes => db.Recipes;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
        db = database;
        db.Build();
    }
}
