using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Restaurant.Models;

public enum OrderStatus
{
    Новый,
    Готовится,
    Готов,
    Оплачен,
    Отменён
}

public partial class Order : ObservableObject
{
    private static int _nextId = 1;

    public int Id { get; }
    public int TableNumber { get; }
    public string WaiterName { get; }
    public DateTime CreatedAt { get; } = DateTime.Now;
    public ObservableCollection<OrderItem> Items { get; } = new();

    [ObservableProperty] private OrderStatus _status = OrderStatus.Новый;

    public decimal Total => Items.Sum(i => i.Subtotal);

    public string DisplayTitle => $"Заказ №{Id} — Стол {TableNumber}";
    public string DisplayInfo => $"{WaiterName} | {CreatedAt:HH:mm} | {Status}";

    public Order(int tableNumber, string waiterName)
    {
        Id = _nextId++;
        TableNumber = tableNumber;
        WaiterName = waiterName;
        Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(Total));
    }

    public void AddItem(MenuItem menuItem, int quantity)
    {
        var existing = Items.FirstOrDefault(i => i.MenuItem.Id == menuItem.Id);
        if (existing != null)
            existing.Quantity += quantity;
        else
            Items.Add(new OrderItem(menuItem, quantity));
        OnPropertyChanged(nameof(Total));
    }

    public void RemoveItem(OrderItem item)
    {
        Items.Remove(item);
        OnPropertyChanged(nameof(Total));
    }

    partial void OnStatusChanged(OrderStatus value)
    {
        OnPropertyChanged(nameof(DisplayInfo));
    }
}
