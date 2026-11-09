using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// Game 어셈블리(게임 규칙) 테스트 — 씬·플레이 없이 계산만 확인

public class PlayerStatusTests
{
    static PlayerStatus NewStatus() =>
        new PlayerStatus { Maxhp = 100, MaxStamina = 100, StaminaRecovery = 15f, SprintCost = 20f, maxCarryWeight = 10f, maxSlots = 10 };

    [Test]
    public void Init하면_최대치로_채움()
    {
        var s = NewStatus();
        s.Init();
        Assert.AreEqual(100f, s.Hp);
        Assert.AreEqual(100f, s.Stamina);
        Assert.AreEqual(s.MaxHungry, s.Hungry);
        Assert.AreEqual(s.MaxWater, s.Water);
    }

    [Test]
    public void 스태미나_0이면_지침_30퍼센트_넘게_차면_풀림()
    {
        var s = NewStatus();
        s.Init();

        s.Stamina = 0;
        Assert.IsTrue(s.isTired);

        s.TickStamina(false, 1f);                       // 15
        Assert.IsTrue(s.isTired);

        s.TickStamina(false, 1.2f);                     // 15 + 18 = 33 → 30% 넘음
        Assert.IsFalse(s.isTired);
    }

    [Test]
    public void 달리면_줄고_가만히_있으면_최대치에서_멈춤()
    {
        var s = NewStatus();
        s.Init();

        s.TickStamina(true, 1f);
        Assert.AreEqual(80f, s.Stamina, 1e-3f);

        s.TickStamina(false, 10f);
        Assert.AreEqual(100f, s.Stamina, 1e-3f);
    }

    [Test]
    public void 물이_30_이하면_최대_스태미나가_줄어듦()
    {
        var s = NewStatus();
        s.Init();
        s.Water = 30;
        Assert.AreEqual(40, s.curMaxStamina);
        s.Water = 31;
        Assert.AreEqual(100, s.curMaxStamina);
    }
}

// 인벤토리·레시피는 ItemManager(아이템 DB)를 쓰므로 테스트용 DB를 만들어 끼움
public abstract class ItemTestBase
{
    protected readonly List<Object> created = new List<Object>();
    protected PlayerStatus status;

    protected ItemBase NewItem(int code, int maxamount, float weight)
    {
        var item = ScriptableObject.CreateInstance<ItemBase>();
        item.name = "item" + code;
        item.itemcode = code;
        item.maxamount = maxamount;
        item.weight = weight;
        created.Add(item);
        return item;
    }

    protected void UseItems(params ItemBase[] items)
    {
        var db = ScriptableObject.CreateInstance<ItemDatabase>();
        created.Add(db);
        var so = new SerializedObject(db);
        var list = so.FindProperty("items");
        list.arraySize = items.Length;
        for (int i = 0; i < items.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        ItemManager.UseDatabase(db);
    }

    [SetUp]
    public void MakeStatus()
    {
        status = new PlayerStatus { maxCarryWeight = 100f, curMaxCarryWeight = 100f, maxSlots = 3 };
    }

    [TearDown]
    public void Cleanup()
    {
        ItemManager.UseDatabase(null);
        foreach (var o in created) if (o != null) Object.DestroyImmediate(o);
        created.Clear();
    }
}

public class InventoryTests : ItemTestBase
{
    [Test]
    public void 한_칸_최대치를_넘으면_다음_칸을_씀()
    {
        var water = NewItem(1, 5, 0.1f);
        UseItems(water);
        var inv = new Inventory(status);

        Assert.IsTrue(inv.AddItem(water, 12, countAsGathered: false));
        Assert.AreEqual(3, inv.UsedSlots);              // 5 + 5 + 2
    }

    [Test]
    public void 칸이_모자라면_AddItem은_하나도_안_넣음()
    {
        var water = NewItem(1, 5, 0.1f);
        UseItems(water);
        var inv = new Inventory(status);

        Assert.IsFalse(inv.AddItem(water, 16, countAsGathered: false));   // 3칸 × 5 = 15까지
        Assert.IsFalse(inv.slots.ContainsKey(1));
    }

    [Test]
    public void AddUpTo는_들어가는_만큼만_넣고_그_수를_돌려줌()
    {
        var water = NewItem(1, 5, 0.1f);
        UseItems(water);
        var inv = new Inventory(status);

        Assert.AreEqual(15, inv.AddUpTo(water, 20, countAsGathered: false));
        Assert.AreEqual(15, inv.slots[1]);
    }

    [Test]
    public void 무게를_넘으면_못_넣음()
    {
        var rock = NewItem(2, 0, 30f);                  // 칸 제한 없음, 하나에 30
        UseItems(rock);
        var inv = new Inventory(status);

        Assert.AreEqual(3, inv.AddUpTo(rock, 5, countAsGathered: false));   // 100 / 30 = 3개
        Assert.AreEqual(90f, inv.currentWeight, 1e-3f);
    }

    [Test]
    public void 빼면_무게와_칸이_돌아옴()
    {
        var water = NewItem(1, 5, 1f);
        UseItems(water);
        var inv = new Inventory(status);
        inv.AddItem(water, 6, countAsGathered: false);

        Assert.IsTrue(inv.RemoveItem(water, 6));
        Assert.AreEqual(0, inv.UsedSlots);
        Assert.AreEqual(0f, inv.currentWeight, 1e-3f);
    }
}

public class RecipeTests : ItemTestBase
{
    RecipeSO NewRecipe(ItemBase result, int resultCount, params (ItemBase item, int amount)[] ingredients)
    {
        var r = ScriptableObject.CreateInstance<RecipeSO>();
        created.Add(r);
        r.result = result;
        r.resultcount = resultCount;
        foreach (var (item, amount) in ingredients) r.ingredients.Add(new RecipeSO.Ingredient { item = item, amount = amount });
        return r;
    }

    [Test]
    public void 재료가_있으면_만들고_재료가_빠짐()
    {
        var cloth = NewItem(10, 10, 0.1f);
        var bandage = NewItem(11, 10, 0.1f);
        UseItems(cloth, bandage);
        var inv = new Inventory(status);
        inv.AddItem(cloth, 3, countAsGathered: false);
        var recipe = NewRecipe(bandage, 1, (cloth, 2));

        Assert.IsTrue(recipe.CanCraft(inv));
        Assert.IsTrue(recipe.Craft(inv));
        Assert.AreEqual(1, inv.slots[10]);
        Assert.AreEqual(1, inv.slots[11]);
    }

    [Test]
    public void 재료가_모자라면_못_만듦()
    {
        var cloth = NewItem(10, 10, 0.1f);
        var bandage = NewItem(11, 10, 0.1f);
        UseItems(cloth, bandage);
        var inv = new Inventory(status);
        inv.AddItem(cloth, 1, countAsGathered: false);
        var recipe = NewRecipe(bandage, 1, (cloth, 2));

        Assert.IsFalse(recipe.CanCraft(inv));
        Assert.IsFalse(recipe.Craft(inv));
        Assert.AreEqual(1, inv.slots[10]);
    }

    [Test]
    public void 결과가_안_들어가면_재료를_돌려놓음()
    {
        var cloth = NewItem(10, 10, 1f);
        var anvil = NewItem(12, 1, 99f);                // 너무 무거워서 못 넣음
        UseItems(cloth, anvil);
        var inv = new Inventory(status);
        inv.AddItem(cloth, 5, countAsGathered: false);
        var recipe = NewRecipe(anvil, 1, (cloth, 2));

        Assert.IsFalse(recipe.Craft(inv));
        Assert.AreEqual(5, inv.slots[10]);
        Assert.AreEqual(5f, inv.currentWeight, 1e-3f);
    }
}
