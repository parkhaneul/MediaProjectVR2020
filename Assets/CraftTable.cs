﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CraftTable : Interactable
{
    public const int craftBoxMaximumSize = 6;

    Item[] GetItemsFromInventory(Inventory inventory)
    {
        if(craftBoxMaximumSize - CraftSystem.Instance.materials.Count >= inventory.getItemsCount())
        {
            return inventory.popAllItems();
        }
        
        Item[] itemsFromInventory = new Item[craftBoxMaximumSize - CraftSystem.Instance.materials.Count];
        for(int i = 0 ; i < itemsFromInventory.Length; i++)
        {
            itemsFromInventory[i] = inventory.popItem(i);
        }
        
        return itemsFromInventory;
    }

    public void Start()
    {
        base.Start();
    }

    private void insertItem(params Item[] items)
    {
        CraftSystem.Instance.addItems(items.Select(_ => _.getData()).ToArray());
    }

    public override void OnInteract(PlayerState state)
    {
        Item[] item = GetItemsFromInventory(state.Inventory);
        insertItem(item);
    }
}
