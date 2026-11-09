using System.Collections.Generic;
using UnityEngine;

public class ItemManager : SingletonBehaviour<ItemManager>
{
    [SerializeField] private ItemDatabase database;

    private static ItemDatabase db;

    public static bool TryGet(int code, out ItemBase item) => db.TryGet(code, out item);
    public static IReadOnlyList<ItemBase> Items => db.Items;
    public static IReadOnlyList<RecipeSO> Recipes => db.Recipes;
    public static IEnumerable<RecipeSO> RecipesFor(ItemType tab) => db.RecipesFor(tab);
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
        UseDatabase(database);
    }

    // 사용할 아이템 DB 지정. 씬에서는 Awake가, 테스트에서는 테스트용 DB로 직접 부름
    public static void UseDatabase(ItemDatabase database)
    {
        db = database;
        if (db != null) db.Build();
    }
}
