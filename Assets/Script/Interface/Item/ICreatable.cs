using UnityEngine;

interface ICreatable
{
    ItemBase iteminfo {  get; }
    bool canCraft(Inventory inventory);
    void Craft(Inventory inventory);
}
