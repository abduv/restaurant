using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Restaurant.Models;

public partial class OrderItem : ObservableObject
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }
    public int MenuItemId { get; set; }

    [ForeignKey(nameof(MenuItemId))]
    public MenuItem MenuItem { get; set; } = null!;

    [ObservableProperty] private int _quantity;

    [NotMapped]
    public decimal Subtotal => MenuItem.Price * Quantity;

    public OrderItem() { }

    public OrderItem(MenuItem menuItem, int quantity)
    {
        MenuItem = menuItem;
        MenuItemId = menuItem.Id;
        _quantity = quantity;
    }

    partial void OnQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(Subtotal));
    }
}
