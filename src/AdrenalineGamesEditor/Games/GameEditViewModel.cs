using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;
using MrCapitalQ.AdrenalineGamesEditor.Shared;
using Windows.Storage.Pickers;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

internal partial class GameEditViewModel : ObservableObject
{
    public readonly static ComboBoxOption<string> CustomExePathOption = new(string.Empty, "Custom");

    private readonly IPackagedAppsService _packagedAppsService;
    private readonly IQualifiedFileResolver _qualifiedFileResolver;
    private readonly IAdrenalineGamesDataService _adrenalineGamesDataService;
    private readonly IMessenger _messenger;
    private readonly IFilePicker _filePicker;
    private readonly IClipboardService _clipboardService;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly IPath _path;
    private readonly ILogger<GameEditViewModel> _logger;

    private string? _autoFixImagePath;
    private string? _autoFixExePath;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _title = "Edit Game";

    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string? _displayName;

    [ObservableProperty]
    private string? _command;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EffectiveImagePath))]
    [NotifyPropertyChangedFor(nameof(ImagePathRequiresAttention))]
    private string? _imagePath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EffectiveImagePath))]
    [NotifyPropertyChangedFor(nameof(ExePathRequiresAttention))]
    private string? _exePath;

    [ObservableProperty]
    private ICollection<ComboBoxOption<string>> _exePathOptions = [];

    [ObservableProperty]
    private ComboBoxOption<string>? _selectedExePathOption;

    [ObservableProperty]
    private bool _isManual;

    [ObservableProperty]
    private bool _isHidden;

    public GameEditViewModel(IPackagedAppsService packagedAppsService,
        IQualifiedFileResolver qualifiedFileResolver,
        IAdrenalineGamesDataService adrenalineGamesDataService,
        IMessenger messenger,
        IFilePicker filePicker,
        IClipboardService clipboardService,
        IDispatcherQueue dispatcherQueue,
        IPath path,
        ILogger<GameEditViewModel> logger,
        string? appUserModelId = null,
        Guid? adrenalineGameId = null)
    {
        _packagedAppsService = packagedAppsService;
        _qualifiedFileResolver = qualifiedFileResolver;
        _adrenalineGamesDataService = adrenalineGamesDataService;
        _messenger = messenger;
        _filePicker = filePicker;
        _clipboardService = clipboardService;
        _dispatcherQueue = dispatcherQueue;
        _path = path;
        _logger = logger;

        _dispatcherQueue.TryEnqueue(async () =>
        {
            IsLoading = true;
            try
            {
                if (appUserModelId is not null)
                    await InitForAppUserModelId(appUserModelId);
                else if (adrenalineGameId is not null)
                    await InitForAdrenalineGameId(adrenalineGameId.Value);
            }
            finally
            {
                IsLoading = false;
            }
        });
    }

    public string? EffectiveImagePath => !string.IsNullOrEmpty(ImagePath) ? ImagePath : ExePath;
    public bool CanAutoFix => _autoFixImagePath is not null || _autoFixExePath is not null;
    public bool ImagePathRequiresAttention => !string.IsNullOrEmpty(ImagePath) && !_path.Exists(ImagePath);
    public bool ExePathRequiresAttention => !string.IsNullOrEmpty(ExePath) && !_path.Exists(ExePath);

    private async Task InitForAppUserModelId(string appUserModelId)
    {
        Title = "New Game";

        var appInfo = await _packagedAppsService.GetInfoAsync(appUserModelId);
        if (appInfo is null)
            return;

        DisplayName = appInfo.DisplayName;
        Command = appUserModelId;
        IsManual = true;
        IsHidden = false;

        ExePathOptions = appInfo.ExecutablePaths
            .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
            .Concat([CustomExePathOption])
            .ToList();

        var exePath = appInfo.ExecutablePath is not null
            ? Path.Combine(appInfo.InstalledPath, appInfo.ExecutablePath)
            : null;
        SetExePath(exePath);

        if (appInfo.Square150x150Logo is not null)
            ImagePath = _qualifiedFileResolver.GetPath(appInfo.InstalledPath, appInfo.Square150x150Logo);
    }

    private async Task InitForAdrenalineGameId(Guid id)
    {
        var game = _adrenalineGamesDataService.GamesData.FirstOrDefault(x => x.Id == id);
        if (game is null)
            return;

        Id = id;
        DisplayName = game.DisplayName;
        Command = game.CommandLine;
        ImagePath = game.ImagePath;
        IsManual = game.IsManual;
        IsHidden = game.IsHidden;

        if (AppUserModelId.TryParse(game.CommandLine, out var aumid)
            && await _packagedAppsService.GetInfoAsync(game.CommandLine) is { } appInfo)
        {
            ExePathOptions = appInfo.ExecutablePaths
                .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
                .Concat([CustomExePathOption])
                .ToList();

            SetExePath(game.ExePath);

            if (!string.IsNullOrEmpty(ExePath) && !_path.Exists(ExePath))
                _autoFixExePath = GetNearestPathMatchInDirectory(ExePath, appInfo.InstalledPath);

            if (!string.IsNullOrEmpty(ImagePath) && !_path.Exists(ImagePath))
                _autoFixImagePath = GetNearestPathMatchInDirectory(ImagePath, appInfo.InstalledPath);

            OnPropertyChanged(nameof(CanAutoFix));
        }
        else
        {
            ExePathOptions = [CustomExePathOption];
            SelectedExePathOption = CustomExePathOption;
            ExePath = game.ExePath;
        }
    }

    private string? GetNearestPathMatchInDirectory(string filePath, string searchDirectoryPath)
    {
        var start = 0;
        while (start < filePath.Length)
        {
            var candidate = Path.Combine(searchDirectoryPath, filePath[start..]);
            if (_path.Exists(candidate))
                return candidate;

            var index = filePath.IndexOf(Path.DirectorySeparatorChar, start);
            if (index < 0)
                break;

            start = index + 1;
        }

        return null;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var gameInfo = new AdrenalineGameInfo(Id,
            DisplayName ?? string.Empty,
            ImagePath ?? string.Empty,
            Command ?? string.Empty,
            ExePath ?? string.Empty,
            IsManual,
            IsHidden);

        try
        {
            await _adrenalineGamesDataService.SaveAsync(gameInfo);

            _messenger.Send(NavigateBackMessage.Instance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while saving a game entry. {GameInfo}", gameInfo);
        }
    }

    [RelayCommand]
    private async Task EditImageAsync()
    {
        var path = await _filePicker.PickSingleFileAsync(PickerLocationId.ComputerFolder, [".jpg", ".jpeg", ".png", ".ico"]);
        if (path is null)
            return;

        ImagePath = path;
    }

    [RelayCommand]
    private void RemoveImage() => ImagePath = null;

    [RelayCommand]
    private void CopyImagePath() => _clipboardService.SetText(ImagePath);

    [RelayCommand]
    private async Task BrowseExeAsync()
    {
        var path = await _filePicker.PickSingleFileAsync(PickerLocationId.ComputerFolder, [".exe"]);
        if (path is null)
            return;

        ExePath = path;
    }

    [RelayCommand]
    private void AutoFix()
    {
        if (_autoFixImagePath is not null)
        {
            ImagePath = _autoFixImagePath;
            _autoFixImagePath = null;
        };

        if (_autoFixExePath is not null)
        {
            SetExePath(_autoFixExePath);
            _autoFixExePath = null;
        }

        OnPropertyChanged(nameof(CanAutoFix));
    }

    private void SetExePath(string? exePath)
    {
        SelectedExePathOption = ExePathOptions.FirstOrDefault(x => x.Value.Equals(exePath, StringComparison.OrdinalIgnoreCase))
            ?? CustomExePathOption;
        ExePath = exePath;
    }

    partial void OnSelectedExePathOptionChanged(ComboBoxOption<string>? value)
    {
        if (value is null)
            ExePath = null;
        else if (value != CustomExePathOption)
            ExePath = value.Value;
    }
}
