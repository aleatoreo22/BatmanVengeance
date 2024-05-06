using System;
using ReactiveUI;
using Tmds.DBus.Protocol;

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

    private string? _adderss;
    public string? Address
    {
        get { return _adderss; }
        set { this.RaiseAndSetIfChanged(ref _adderss, value); }
    }

    private int? _len;
    public int? Len
    {
        get { return _len; }
        set { this.RaiseAndSetIfChanged(ref _len, value); }
    }

    private string? _textTranslated;
    public string? TextTranslated
    {
        get { return _textTranslated; }
        set
        {
            LenTranslated = System.Text.Encoding.UTF8.GetByteCount(value ?? "");
            AddressTranslated = Len >= LenTranslated ? Address : "TO BE CALC";
            this.RaiseAndSetIfChanged(ref _textTranslated, value);
        }
    }

    private int? _lenTranslated = 0;
    public int? LenTranslated
    {
        get { return _lenTranslated; }
        set { this.RaiseAndSetIfChanged(ref _lenTranslated, value); }
    }

    private string? _addressTranslated;
    public string? AddressTranslated
    {
        get { return _addressTranslated; }
        set { this.RaiseAndSetIfChanged(ref _addressTranslated, value); }
    }

}