using ReactiveUI;

namespace BatmanVengeance.Frontend.ViewModels;

public class ItemControllerViewModel : ViewModelBase
{
    private string? _text;
    public string? Text
    {
        get { return _text; }
        set { this.RaiseAndSetIfChanged(ref _text, value); }
    }

    private bool _visible = true;
    public bool Visible
    {
        get { return _visible; }
        set { this.RaiseAndSetIfChanged(ref _visible, value); }
    }

    private string? _adderssString;
    public string? AddressString
    {
        get { return _adderssString; }
        set { this.RaiseAndSetIfChanged(ref _adderssString, value); }
    }

    private string? _len;
    public string? Len
    {
        get { return _len; }
        set { this.RaiseAndSetIfChanged(ref _len, value); }
    }

    public int AddressInt { get; set; }
}