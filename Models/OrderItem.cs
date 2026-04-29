using CommunityToolkit.Mvvm.ComponentModel;

namespace Restaurant.Models;

public partial class OrderItem : ObservableObject
{
    public MenuItem MenuItem { get; }

    [ObservableProperty] private int _quantity;

    public decimal Subtotal => MenuItem.Price * Quantity;

    public OrderItem(MenuItem menuItem, int quantity)
    {
        MenuItem = menuItem;
        _quantity = quantity;
    }

    partial void OnQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(Subtotal));
    }
}
