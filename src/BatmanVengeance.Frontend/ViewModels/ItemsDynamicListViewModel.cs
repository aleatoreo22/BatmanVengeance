using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Templates;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BatmanVengeance.Frontend.ViewModels;

public class ItemsDynamicListViewModel : ViewModelBase
{
    private TemplatedControl? _itemsSource;
    public TemplatedControl? ItemsSource
    {
        get { return _itemsSource; }
        set { _itemsSource = value; }
    }
    
}
