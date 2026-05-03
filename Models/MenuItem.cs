using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Restaurant.Models;

public enum Category
{
    Закуски,
    Салаты,
    Супы,
    Горячее,
    Десерты,
    Напитки
}

public partial class MenuItem : ObservableObject
{
    [Key]
    public int Id { get; set; }

    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private decimal _price;
    [ObservableProperty] private Category _category;
    [ObservableProperty] private bool _isAvailable = true;

    public MenuItem() { }

    public MenuItem(string name, string description, decimal price, Category category)
    {
        _name = name;
        _description = description;
        _price = price;
        _category = category;
    }
}
