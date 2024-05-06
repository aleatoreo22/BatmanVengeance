using BatmanVengeance.Frontend.Contollers;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using BatmanVengeance.Core;
using System.ComponentModel;
using BatmanVengeance.Model;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Controls;

namespace BatmanVengeance.Frontend.ViewModels;

#region  Enum
enum PageFunction
{
    next,
    previous
}
#endregion

public class MainWindowViewModel : ViewModelBase
{
    #region Const
    private const int ITEMS_PER_PAGE = 5;
    #endregion

    #region  Props
    public int TotalPages { get; set; }

    public List<ItemControllerViewModel>? ItemsControllerViewModel { get; set; }

    private ObservableCollection<ItemController>? _itemsController;

    public ObservableCollection<ItemController>? ItemsController
    {
        get { return _itemsController; }
        set { this.RaiseAndSetIfChanged(ref _itemsController, value); }
    }

    private int _page;
    public int Page
    {
        get { return _page; }
        set { this.RaiseAndSetIfChanged(ref _page, value); }
    }

    public ICommand? SelectFile { get; }

    public ICommand? ChangePage { get; }

    private string? _fileSelected;
    public string? FileSelected
    {
        get { return _fileSelected; }
        set { this.RaiseAndSetIfChanged(ref _fileSelected, value); }
    }

    public string? FileSelectedFullPath { get; set; }

    public List<HexInfo>? HexInfos { get; set; }
    #endregion

    #region  Ctor
    public MainWindowViewModel()
    {
        SelectFile = ReactiveCommand.Create(async () =>
        {
            await OpenFile();
            ReadFile();
        });

        ChangePage = ReactiveCommand.Create((PageFunction pageFunction) =>
        {
            ShowItensController(Page + ((pageFunction == PageFunction.next) ? 1 : -1));
        });
    }
    #endregion

    #region Meths
    private void ReadFile()
    {
        var worker = new BackgroundWorker();
        worker.DoWork += (x, y) =>
        {
            y.Result = new HexReader(FileSelectedFullPath ?? "", 0,
             blackList:
             [
                "2e",
                "23",
                "00",
                "e4",
                "ff",
                "9a",
                "84",
                "82",
                "11",
                "8b",
                "98",
                "81",
                "7f",
                "19",
                "93",
                "10",
                "85",
                "94",
                "8a",
                "13",
                "9f",
                "97",
                "03",
                "1d",
                "04",
                "0e",
                "95",
                "80",
                "01",
                "90",
                "88",
                "87",
                "07",
                "96",
                "12",
                "1f",
                "18",
                "1c",
                "0f",
                "05",
                "08",
                "86",
                "02",
                "9e",
                "14",
                "0c",
                "83",
                "1e",
                "17",
                "1b",
                "16",
                "15",
                "0b",
                "06",
                "1a",
                "8c",
                "92",
                "8e",
                "89",
                "91",
                "9b",
                "9c",
                "8f",
                "99",
                "9d",
                "8d"

             ]).Read();
        };
        worker.RunWorkerCompleted += (x, y) =>
        {
            if (y.Error != null)
            {
                Console.WriteLine(y.Error.Message);
                return;
            }
            ItemsControllerViewModel = ((List<HexInfo>?)y.Result ?? [])
            .Select(a => new ItemControllerViewModel
            {
                AddressString = a.Position,
                Len = a.ValueString.Length.ToString(),
                Text = a.ValueString,
                AddressInt = int.Parse(a.Position, System.Globalization.NumberStyles.HexNumber),
                Visible = true
            }).ToList();
            TotalPages = (int)Math.Ceiling((double)ItemsControllerViewModel.Count / ITEMS_PER_PAGE);
            ShowItensController(1);
        };
        worker.RunWorkerAsync();
    }

    private async Task OpenFile()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
        desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open Rom File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new("GBA File")
                {
                    Patterns = [ "*.gba" ]
                }
            ]
        });
        FileSelected = files.FirstOrDefault()?.Name ?? "";
        FileSelectedFullPath = files.FirstOrDefault()?.Path.LocalPath ?? "";
    }

    public void ShowItensController(int page)
    {
        if (page < 1)
            page = 1;
        if (page > TotalPages)
            page = TotalPages;

        if (TotalPages > 0 && page > 0)
        {
            var index = (page - 1) * ITEMS_PER_PAGE;
            List<ItemController> itemsController = [];
            for (int i = index; i < index + ITEMS_PER_PAGE; i++)
            {
                if (i + 1 > ItemsControllerViewModel?.Count)
                    continue;
                itemsController.Add(new ItemController
                {
                    DataContext = ItemsControllerViewModel?[i],
                });
            }
            ItemsController = new ObservableCollection<ItemController>(itemsController);
            Page = page;
        }
    }
    #endregion
}
