using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    [Key]
    public int Id { get; set; }

    public int TableNumber { get; set; }
    public string WaiterName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ObservableCollection<OrderItem> Items { get; set; } = new();

    [ObservableProperty] private OrderStatus _status = OrderStatus.Новый;

    [NotMapped]
    public decimal Total => Items.Sum(i => i.Subtotal);

    [NotMapped]
    public string DisplayTitle => $"Заказ №{Id} — Стол {TableNumber}";

    [NotMapped]
    public string DisplayInfo => $"{WaiterName} | {CreatedAt:HH:mm} | {Status}";

    public Order()
    {
        Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(Total));
    }

    public Order(int tableNumber, string waiterName) : this()
    {
        TableNumber = tableNumber;
        WaiterName = waiterName;
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
