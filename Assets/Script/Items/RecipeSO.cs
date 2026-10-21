using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeSO", menuName = "Scriptable Objects/RecipeSO")]
public class RecipeSO : ScriptableObject
{
    [System.Serializable]
    public struct Ingrediednt
    {
        public ItemBase item;
        public int amount;
    }

    public ItemBase result;
    public int resultcount = 1;
    public List<Ingrediednt> ingredients = new();

    public bool CanCraft(Inventory inventory)
    {
        foreach (var material in ingredients)
        {
            if (material.item == null) return false;
            if (!inventory.slots.TryGetValue(material.item.itemcode, out int count)) return false;
            if (count < material.amount) return false;
        }
        return true;
    }
    public bool Craft(Inventory inventory)
    {
        if (!CanCraft(inventory)) return false;

        foreach (var material in ingredients)
            inventory.RemoveItem(material.item, material.amount);

        if (inventory.AddItem(result,resultcount)) return true;

        foreach (var material in ingredients)
            inventory.RestoreItem(material.item, material.amount);
        return false;
    }
}
