using System.Collections.Generic;
using BatmanVengeance.Frontend.ViewModels;

namespace BatmanVengeance.Frontend.Models;

public class Project
{
    public int TotalPages { get; set; }

    public List<ItemControllerViewModel>? ItemsControllerViewModel { get; set; }

    public string? FileSelected { get; set; }

    public string? FileSelectedFullPath { get; set; }
}