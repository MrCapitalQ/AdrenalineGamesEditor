using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Shared;
using Windows.Storage.Pickers;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

internal partial class GameEditViewModel : ObservableObject, IAdrenalineGameImageInfo
{
    public readonly static ComboBoxOption<string> CustomExePathOption = new("!CUSTOM!", "Custom");

    private readonly IPackagedAppsService _packagedAppsService;
    private readonly IQualifiedFileResolver _qualifiedFileResolver;
    private readonly IAdrenalineGamesDataService _adrenalineGamesDataService;
    private readonly IMessenger _messenger;
    private readonly IFilePicker _filePicker;
    private readonly IClipboardService _clipboardService;
    private readonly ILogger<GameEditViewModel> _logger;

    [ObservableProperty]
    private string _title = "Edit Game";

    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string? _displayName;

    [ObservableProperty]
    private string? _command;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GameImage))]
    [NotifyPropertyChangedFor(nameof(HasImagePath))]
    private string? _imagePath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GameImage))]
    private string? _exePath;

    [ObservableProperty]
    private IEnumerable<ComboBoxOption<string>> _exePathOptions = [];

    [ObservableProperty]
    private ComboBoxOption<string>? _selectedExePathOption;

    public GameEditViewModel(IPackagedAppsService packagedAppsService,
        IQualifiedFileResolver qualifiedFileResolver,
        IAdrenalineGamesDataService adrenalineGamesDataService,
        IMessenger messenger,
        IFilePicker filePicker,
        IClipboardService clipboardService,
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
        _logger = logger;

        GameImage = new(this);

        if (appUserModelId is not null)
            _ = InitForAppUserModelId(appUserModelId);
        else if (adrenalineGameId is not null)
            _ = InitForAdrenalineGameId(adrenalineGameId.Value);
    }

    public GameImageAdapter GameImage { get; }

    public bool HasImagePath => !string.IsNullOrEmpty(ImagePath);

    private async Task InitForAppUserModelId(string appUserModelId)
    {
        Title = "New Game";

        var appInfo = await _packagedAppsService.GetInfoAsync(appUserModelId);
        if (appInfo is null)
            return;

        DisplayName = appInfo.DisplayName;
        Command = appUserModelId;

        var exePath = appInfo.ExecutablePath is not null
            ? Path.Combine(appInfo.InstalledPath, appInfo.ExecutablePath)
            : null;
        ExePathOptions = appInfo.ExecutablePaths
            .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
            .Concat([CustomExePathOption])
            .ToList();

        SelectedExePathOption = ExePathOptions.FirstOrDefault(x => x.Value == exePath)
            ?? CustomExePathOption;

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

        if (AppUserModelId.TryParse(game.CommandLine, out var _)
            && await _packagedAppsService.GetInfoAsync(game.CommandLine) is { } appInfo)
        {
            ExePathOptions = appInfo.ExecutablePaths
                .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
                .Concat([CustomExePathOption])
                .ToList();

            SelectedExePathOption = ExePathOptions.FirstOrDefault(x => x.Value == game.ExePath)
                ?? CustomExePathOption;
        }
        else
        {
            ExePathOptions = [CustomExePathOption];
            SelectedExePathOption = CustomExePathOption;
            ExePath = game.ExePath;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var gameInfo = new AdrenalineGameInfo(Id,
            DisplayName ?? string.Empty,
            ImagePath ?? string.Empty,
            Command ?? string.Empty,
            ExePath ?? string.Empty,
            true);

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

    [RelayCommand(CanExecute = nameof(HasImagePath))]
    private void CopyImagePath() => _clipboardService.SetText(ImagePath);

    partial void OnSelectedExePathOptionChanged(ComboBoxOption<string>? value)
    {
        if (value is null)
            ExePath = null;
        else if (value != CustomExePathOption)
            ExePath = value.Value;
    }

    internal class GameImageAdapter(GameEditViewModel viewModel) : IAdrenalineGameImageInfo
    {
        private readonly GameEditViewModel _viewModel = viewModel;

        public string? ImagePath => _viewModel.ImagePath;
        public string? ExePath => _viewModel.ExePath;
    }
}
