using System;
using System.Collections.Generic;
using UnityEngine;



public class Item : MonoBehaviour
{
    public enum ItemID
    {
        Bottle,
        Can,
        Shit
    }
    public event Action OnItemPicked;
    public event Action OnVisualRefresh;

    [SerializeField] private ItemID id;
    public int Id => (int)id;

    public void Pick() => OnItemPicked?.Invoke();
    public void RefreshVisual() => OnVisualRefresh?.Invoke();
}