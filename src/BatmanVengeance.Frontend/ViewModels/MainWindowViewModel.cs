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
using System.IO;
using System.Text.Json;
using BatmanVengeance.Frontend.Models;

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
    private const int ITEMS_PER_PAGE = 8;
    #endregion

    #region  Props
    public string? SavePath { get; set; }

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

    public ICommand? SaveProject { get; }

    private string? _fileSelected;
    public string? FileSelected
    {
        get { return _fileSelected; }
        set { this.RaiseAndSetIfChanged(ref _fileSelected, value); }
    }

    public string? FileSelectedFullPath { get; set; }

    public Project? Project { get; set; }
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

        SaveProject = ReactiveCommand.Create(async () =>
        {
            if (string.IsNullOrEmpty(SavePath))
                await ChooseSavePath();
            SaveFile();
        });
    }


    #endregion

    #region Meths
    private void SaveFile()
    {
        if (string.IsNullOrEmpty(SavePath))
            return;
        File.WriteAllTextAsync(SavePath, JsonSerializer.Serialize(Project));
    }

    private async Task ChooseSavePath()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");
        var files = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save project",
            SuggestedFileName = "BatmanVengeance.json"
        }

       );
        SavePath = files?.Path.AbsolutePath ?? "";
    }

    private void ReadFile()
    {
        var worker = new BackgroundWorker();
        worker.DoWork += (x, y) =>
        {
            y.Result = FileSelected?.Substring(FileSelected.LastIndexOf(".") + 1) == "json" ?
             ReadProjectFile() :
             new HexReader(FileSelectedFullPath ?? "", 3,
             blackList:
             [
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
                "8d",
                "0d"

             ], positionDelimiters: [
                new PositionDelimiter{
                    InitialPosition = Convert.ToInt64("6CAE0", 16),
                    EndPosition = Convert.ToInt64("6E7AF", 16),
                },
                new PositionDelimiter {
                    InitialPosition = Convert.ToInt64("69933", 16),
                    EndPosition = Convert.ToInt64("69E25", 16),
                }
             ]).Read();
        };
        worker.RunWorkerCompleted += (x, y) =>
        {
            if (y.Error != null)
            {
                Console.WriteLine(y.Error.Message);
                return;
            }
            if (y.Result == null)
                return;
            if (typeof(List<HexInfo>) == y?.Result?.GetType())
            {
                var itemsControllerViewModel = ((List<HexInfo>?)y.Result ?? []).OrderByDescending(x => Convert.ToInt64(x.Position, 16))
                .Select(a => new ItemControllerViewModel
                {
                    Address = a.Position,
                    Len = a.Len,
                    Text = a.ValueString,
                    Visible = true
                }).ToList();

                Project = new Project
                {
                    FileSelected = FileSelected,
                    FileSelectedFullPath = FileSelectedFullPath,
                    ItemsControllerViewModel = itemsControllerViewModel,
                    TotalPages = (int)Math.Ceiling((double)itemsControllerViewModel.Count / ITEMS_PER_PAGE)
                };

            }
            else
            {
                Project = (Project?)y?.Result;
                FileSelected = Project?.FileSelected;
                FileSelectedFullPath = Project?.FileSelectedFullPath;
            }
            ShowItensController(1);
        };
        worker.RunWorkerAsync();
    }

    private Project? ReadProjectFile()
    {
        var project = JsonSerializer.Deserialize<Project>(File.ReadAllText(FileSelectedFullPath ?? ""));
        SavePath = FileSelectedFullPath;
        return project;
    }

    private async Task OpenFile()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
        desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");
        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open rom file or a project",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new("GBA File")
                {
                    Patterns = [ "*.gba" ]
                },
                  new("Project")
                {
                    Patterns = ["*.json"]
                }
            ],

        });
        FileSelected = files.FirstOrDefault()?.Name ?? "";
        FileSelectedFullPath = files.FirstOrDefault()?.Path.LocalPath ?? "";
    }

    public void ShowItensController(int page)
    {
        if (page < 1)
            page = 1;
        if (page > Project?.TotalPages)
            page = Project.TotalPages;

        if (Project?.TotalPages > 0 && page > 0)
        {
            var index = (page - 1) * ITEMS_PER_PAGE;
            List<ItemController> itemsController = [];
            for (int i = index; i < index + ITEMS_PER_PAGE; i++)
            {
                if (i + 1 > Project.ItemsControllerViewModel?.Count)
                    continue;
                itemsController.Add(new ItemController
                {
                    DataContext = Project.ItemsControllerViewModel?[i],
                });
            }
            ItemsController = new ObservableCollection<ItemController>(itemsController);
            Page = page;
        }
    }
    #endregion
}
