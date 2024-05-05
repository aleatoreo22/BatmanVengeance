using BatmanVengeance.Frontend.Contollers;
using ReactiveUI;
using System;
using System.IO;
using System.Threading;
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
using DynamicData.Kernel;

namespace BatmanVengeance.Frontend.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public List<ItemControllerViewModel>? ItensControllerViewModel { get; set; }
    private ObservableCollection<ItemController>? _itensController;
    public ObservableCollection<ItemController>? ItensController
    {
        get { return _itensController; }
        set { this.RaiseAndSetIfChanged(ref _itensController, value); }
    }

    public ICommand? SelectFile { get; }

    private string? _fileSelected;
    public string? FileSelected
    {
        get { return _fileSelected; }
        set { this.RaiseAndSetIfChanged(ref _fileSelected, value); }
    }

    private string? _log;
    public string? Log
    {
        get { return _log; }
        set { this.RaiseAndSetIfChanged(ref _log, value); }
    }

    public string? FileSelectedFullPath { get; set; }

    public List<HexInfo>? HexInfos { get; set; }

    public MainWindowViewModel()
    {
        SelectFile = ReactiveCommand.Create(async () =>
        {
            await OpenFile();
            ReadFile();
        });
    }

    private void ReadFile()
    {
        var worker = new BackgroundWorker();
        worker.DoWork += (x, y) =>
        {
            y.Result = new HexReader(FileSelectedFullPath ?? "", blackList:
            [
                "2e",
                "00",
                "e4",
                "ff"
            ]).Read();
        };
        worker.RunWorkerCompleted += (x, y) =>
        {
            if (y.Error != null)
            {
                Log += y.Error.Message;
                return;
            }
            ItensControllerViewModel = ((List<HexInfo>?)y.Result ?? [])
            .Select(a => new ItemControllerViewModel
            {
                AddressString = a.Position,
                Len = a.ValueString.Length.ToString(),
                Text = a.ValueString,
                AddressInt = int.Parse(a.Position, System.Globalization.NumberStyles.HexNumber),
                Visible = false
            }).ToList();

            ItensController = new ObservableCollection<ItemController>(
                ItensControllerViewModel.GetRange(0, 500).Select(
                    a => new ItemController
                    {
                        DataContext = a
                    }
                ).ToList()
            );
            ShowItensController();
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

    internal void ShowItensController()
    {
        int index;
        var lastItem = ItensController?.LastOrDefault
        (
            x => ((ItemControllerViewModel?)x.DataContext)?.Visible ?? false
        );
        if (lastItem != null)
            index = ItensController?.IndexOf(lastItem) + 1 ?? 0;
        else
            index = 0;



        for (int i = index; i < index + 200; i++)
        {
            if ((ItensController?[i].DataContext) != null)
                ((ItemControllerViewModel)ItensController[i].DataContext).Visible = true;
        }

        if (index < 500)
            return;
        for (int i = 0; i < index - 500; i++)
        {
            if ((ItensController?[i].DataContext) != null)
                ((ItemControllerViewModel)ItensController[i].DataContext).Visible = false;
        }
    }

    internal void OnScrollChanged(ref ScrollViewer scrollViewer, double? y, double height)
    {
        if (y == 0 || y == null)
            return;
        if (height - y <= 10)
        {

            if (ItensController?.ToList()
                    .FindAll(x => ((ItemControllerViewModel?)x?.DataContext)?.Visible ?? false)
                    .Count >= 500)
            {
                ShowItensController();
                scrollViewer.Offset = new Vector(scrollViewer.Offset.X, (double)y - 3900);
            }
        }
    }
}
