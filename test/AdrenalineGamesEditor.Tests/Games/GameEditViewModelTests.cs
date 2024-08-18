using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;
using MrCapitalQ.AdrenalineGamesEditor.Games;
using MrCapitalQ.AdrenalineGamesEditor.Shared;
using NSubstitute.ExceptionExtensions;
using Windows.Storage.Pickers;

namespace MrCapitalQ.AdrenalineGamesEditor.Tests.Games;

public class GameEditViewModelTests
{
    private readonly IPackagedAppsService _packagedAppsService = Substitute.For<IPackagedAppsService>();
    private readonly IQualifiedFileResolver _qualifiedFileResolver = Substitute.For<IQualifiedFileResolver>();
    private readonly IAdrenalineGamesDataService _adrenalineGamesDataService = Substitute.For<IAdrenalineGamesDataService>();
    private readonly IMessenger _messenger = Substitute.For<IMessenger>();
    private readonly IFilePicker _filePicker = Substitute.For<IFilePicker>();
    private readonly IClipboardService _clipboardService = Substitute.For<IClipboardService>();
    private readonly IDispatcherQueue _dispatcherQueue = Substitute.For<IDispatcherQueue>();
    private readonly IPath _path = Substitute.For<IPath>();
    private readonly FakeLogger<GameEditViewModel> _logger = new();

    private readonly GameEditViewModel _viewModel;

    public GameEditViewModelTests()
    {
        _dispatcherQueue.TryEnqueue(Arg.Any<Action>()).Returns(x =>
        {
            x.Arg<Action>().Invoke();
            return true;
        });
        _path.Exists(Arg.Any<string>()).Returns(true);

        _viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger);
    }

    [Fact]
    public void Ctor_ForAppUserModelId_InitializesForAppUserModelId()
    {
        // Arrange
        var appInfo = new PackagedAppInfo("Test Game",
            "Package!App",
            @"C:\InstalledLocation",
            DateTimeOffset.MinValue,
            null,
            @"Assets\Logo.png",
            true,
            "Game.exe",
            ["gamelauncherhelper.exe", "Game.exe"]);
        var expectedExePath = Path.Combine(appInfo.InstalledPath, appInfo.ExecutablePath!);
        var expectedImagePath = Path.Combine(appInfo.InstalledPath, appInfo.Square150x150Logo!);
        var expectedOptions = appInfo.ExecutablePaths
            .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
            .Concat([GameEditViewModel.CustomExePathOption]);
        _packagedAppsService.GetInfoAsync(appInfo.AppUserModelId).Returns(appInfo);
        _qualifiedFileResolver.GetPath(appInfo.InstalledPath, appInfo.Square150x150Logo!)
            .Returns(Path.Combine(appInfo.InstalledPath, appInfo.Square150x150Logo!));

        // Act
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger,
            appUserModelId: appInfo.AppUserModelId);

        // Assert
        Assert.Equal("New Game", viewModel.Title);
        Assert.Equal(Guid.Empty, viewModel.Id);
        Assert.Equal(appInfo.DisplayName, viewModel.DisplayName);
        Assert.Equal(appInfo.AppUserModelId, viewModel.Command);
        Assert.Equal(expectedImagePath, viewModel.ImagePath);
        Assert.Equal(expectedExePath, viewModel.ExePath);
        Assert.Equal(expectedOptions, viewModel.ExePathOptions);
        Assert.Equal(viewModel.ExePathOptions.Single(x => x.Value == expectedExePath), viewModel.SelectedExePathOption);
        Assert.True(viewModel.IsManual);
        Assert.False(viewModel.IsHidden);
        Assert.False(viewModel.CanAutoFix);
        Assert.False(viewModel.ImagePathRequiresAttention);
        Assert.False(viewModel.ExePathRequiresAttention);
    }

    [Fact]
    public void Ctor_ForAppUserModelIdWithNoExePath_InitializesWithoutExePath()
    {
        // Arrange
        var appInfo = new PackagedAppInfo("Test Game",
            "Package!App",
            @"C:\InstalledLocation",
            DateTimeOffset.MinValue,
            null,
            @"Assets\Logo.png",
            true,
            null,
            []);
        var expectedImagePath = Path.Combine(appInfo.InstalledPath, appInfo.Square150x150Logo!);
        var expectedOptions = appInfo.ExecutablePaths
            .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
            .Concat([GameEditViewModel.CustomExePathOption]);
        _packagedAppsService.GetInfoAsync(appInfo.AppUserModelId).Returns(appInfo);
        _qualifiedFileResolver.GetPath(appInfo.InstalledPath, appInfo.Square150x150Logo!)
            .Returns(Path.Combine(appInfo.InstalledPath, appInfo.Square150x150Logo!));

        // Act
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger,
            appUserModelId: appInfo.AppUserModelId);

        // Assert
        Assert.Equal("New Game", viewModel.Title);
        Assert.Equal(Guid.Empty, viewModel.Id);
        Assert.Equal(appInfo.DisplayName, viewModel.DisplayName);
        Assert.Equal(appInfo.AppUserModelId, viewModel.Command);
        Assert.Equal(expectedImagePath, viewModel.ImagePath);
        Assert.Null(viewModel.ExePath);
        Assert.Equal(expectedOptions, viewModel.ExePathOptions);
        Assert.Equal(GameEditViewModel.CustomExePathOption, viewModel.SelectedExePathOption);
        Assert.True(viewModel.IsManual);
        Assert.False(viewModel.IsHidden);
        Assert.False(viewModel.CanAutoFix);
        Assert.False(viewModel.ImagePathRequiresAttention);
        Assert.False(viewModel.ExePathRequiresAttention);
    }

    [Fact]
    public void Ctor_AppUserModelIdNotFound_DoesNotInitialize()
    {
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger,
            appUserModelId: "Package!App");

        Assert.Equal("New Game", viewModel.Title);
        Assert.Equal(Guid.Empty, viewModel.Id);
        Assert.Null(viewModel.DisplayName);
        Assert.Null(viewModel.Command);
        Assert.Null(viewModel.ImagePath);
        Assert.Null(viewModel.ExePath);
        Assert.Empty(viewModel.ExePathOptions);
        Assert.Null(viewModel.SelectedExePathOption);
        Assert.False(viewModel.IsManual);
        Assert.False(viewModel.IsHidden);
        Assert.False(viewModel.CanAutoFix);
        Assert.False(viewModel.ImagePathRequiresAttention);
        Assert.False(viewModel.ExePathRequiresAttention);
    }

    [Fact]
    public void Ctor_ForAdrenalineGameId_InitializesForAdrenalineGameId()
    {
        // Arrange
        var expectedOptions = new List<ComboBoxOption<string>> { GameEditViewModel.CustomExePathOption };
        var gameInfo = new AdrenalineGameInfo(Guid.Parse("9721c8ae-b162-4e64-bbc6-eac6d933b711"),
            "Test Display Name",
            @"C:\Path\Image.png",
            "Test-Command",
            @"C:\Path\Executable.exe",
            true,
            true);
        _adrenalineGamesDataService.GamesData.Returns([gameInfo]);

        // Act
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger,
            adrenalineGameId: gameInfo.Id);

        // Assert
        Assert.Equal("Edit Game", viewModel.Title);
        Assert.Equal(gameInfo.Id, viewModel.Id);
        Assert.Equal(gameInfo.DisplayName, viewModel.DisplayName);
        Assert.Equal(gameInfo.CommandLine, viewModel.Command);
        Assert.Equal(gameInfo.ImagePath, viewModel.ImagePath);
        Assert.Equal(gameInfo.ExePath, viewModel.ExePath);
        Assert.Equal(expectedOptions, viewModel.ExePathOptions);
        Assert.Equal(GameEditViewModel.CustomExePathOption, viewModel.SelectedExePathOption);
        Assert.Equal(gameInfo.IsManual, viewModel.IsManual);
        Assert.Equal(gameInfo.IsHidden, viewModel.IsHidden);
        Assert.False(viewModel.CanAutoFix);
        Assert.False(viewModel.ImagePathRequiresAttention);
        Assert.False(viewModel.ExePathRequiresAttention);
    }

    [Fact]
    public void Ctor_ForAdrenalineGameIdAndIsPackagedApp_InitializesWithPackageInfo()
    {
        // Arrange
        var appInfo = new PackagedAppInfo("Test Game",
            "Package!App",
            @"C:\InstalledLocation",
            DateTimeOffset.MinValue,
            null,
            @"Assets\Logo.png",
            true,
            "Game.exe",
            ["gamelauncherhelper.exe", "Game.exe"]);
        var expectedOptions = appInfo.ExecutablePaths
            .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
            .Concat([GameEditViewModel.CustomExePathOption]);
        _packagedAppsService.GetInfoAsync(appInfo.AppUserModelId).Returns(appInfo);
        var gameInfo = new AdrenalineGameInfo(Guid.Parse("9721c8ae-b162-4e64-bbc6-eac6d933b711"),
            "Test Display Name",
            @"C:\Path\Image.png",
            appInfo.AppUserModelId,
            @"C:\InstalledLocation\Game.exe",
            true,
            true);
        _adrenalineGamesDataService.GamesData.Returns([gameInfo]);

        // Act
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger,
            adrenalineGameId: gameInfo.Id);

        // Assert
        Assert.Equal("Edit Game", viewModel.Title);
        Assert.Equal(gameInfo.Id, viewModel.Id);
        Assert.Equal(gameInfo.DisplayName, viewModel.DisplayName);
        Assert.Equal(gameInfo.CommandLine, viewModel.Command);
        Assert.Equal(gameInfo.ImagePath, viewModel.ImagePath);
        Assert.Equal(gameInfo.ExePath, viewModel.ExePath);
        Assert.Equal(expectedOptions, viewModel.ExePathOptions);
        Assert.Equal(viewModel.ExePathOptions.Single(x => x.Value == gameInfo.ExePath), viewModel.SelectedExePathOption);
        Assert.False(viewModel.CanAutoFix);
        Assert.False(viewModel.ImagePathRequiresAttention);
        Assert.False(viewModel.ExePathRequiresAttention);
    }

    [Fact]
    public void Ctor_AdrenalineGameIdNotFound_DoesNotInitialize()
    {
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger,
            adrenalineGameId: Guid.NewGuid());

        Assert.Equal("Edit Game", viewModel.Title);
        Assert.Equal(Guid.Empty, viewModel.Id);
        Assert.Null(viewModel.DisplayName);
        Assert.Null(viewModel.Command);
        Assert.Null(viewModel.ImagePath);
        Assert.Null(viewModel.ExePath);
        Assert.Empty(viewModel.ExePathOptions);
        Assert.Null(viewModel.SelectedExePathOption);
        Assert.False(viewModel.CanAutoFix);
        Assert.False(viewModel.ImagePathRequiresAttention);
        Assert.False(viewModel.ExePathRequiresAttention);
    }

    [Fact]
    public void Ctor_IsPackagedAppWithNoPaths_CannotAutoFix()
    {
        // Arrange
        var appInfo = new PackagedAppInfo("Test Game",
            "Package!App",
            @"C:\InstalledLocation\Package_v2",
            DateTimeOffset.MinValue,
            null,
            @"Assets\Logo.png",
            true,
            "Game.exe",
            ["gamelauncherhelper.exe", "Game.exe"]);
        var expectedOptions = appInfo.ExecutablePaths
            .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
            .Concat([GameEditViewModel.CustomExePathOption]);
        _packagedAppsService.GetInfoAsync(appInfo.AppUserModelId).Returns(appInfo);
        var gameInfo = new AdrenalineGameInfo(Guid.Parse("9721c8ae-b162-4e64-bbc6-eac6d933b711"),
            "Test Display Name",
            string.Empty,
            appInfo.AppUserModelId,
            string.Empty,
            true,
            true);
        _adrenalineGamesDataService.GamesData.Returns([gameInfo]);
        _packagedAppsService.GetInfoAsync(gameInfo.CommandLine).Returns(appInfo);
        _path.Exists(gameInfo.ImagePath).Returns(false);

        // Act
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger,
            adrenalineGameId: gameInfo.Id);

        // Assert
        Assert.Equal("Edit Game", viewModel.Title);
        Assert.Equal(gameInfo.Id, viewModel.Id);
        Assert.Equal(gameInfo.DisplayName, viewModel.DisplayName);
        Assert.Equal(gameInfo.CommandLine, viewModel.Command);
        Assert.Equal(gameInfo.ImagePath, viewModel.ImagePath);
        Assert.Equal(gameInfo.ExePath, viewModel.ExePath);
        Assert.Equal(expectedOptions, viewModel.ExePathOptions);
        Assert.Equal(GameEditViewModel.CustomExePathOption, viewModel.SelectedExePathOption);
        Assert.False(viewModel.CanAutoFix);
        Assert.False(viewModel.ImagePathRequiresAttention);
        Assert.False(viewModel.ExePathRequiresAttention);
    }

    [Fact]
    public void Ctor_IsPackagedAppWithBrokenImagePathAndAutoFixCandidate_CanAutoFixImagePath()
    {
        // Arrange
        var appInfo = new PackagedAppInfo("Test Game",
            "Package!App",
            @"C:\InstalledLocation\Package_v2",
            DateTimeOffset.MinValue,
            null,
            @"Assets\Logo.png",
            true,
            "Game.exe",
            ["gamelauncherhelper.exe", "Game.exe"]);
        var expectedOptions = appInfo.ExecutablePaths
            .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
            .Concat([GameEditViewModel.CustomExePathOption]);
        _packagedAppsService.GetInfoAsync(appInfo.AppUserModelId).Returns(appInfo);
        var gameInfo = new AdrenalineGameInfo(Guid.Parse("9721c8ae-b162-4e64-bbc6-eac6d933b711"),
            "Test Display Name",
            @"C:\InstalledLocation\Package_v1\Image.png",
            appInfo.AppUserModelId,
            @"C:\InstalledLocation\Package_v1\Game.exe",
            true,
            true);
        _adrenalineGamesDataService.GamesData.Returns([gameInfo]);
        _packagedAppsService.GetInfoAsync(gameInfo.CommandLine).Returns(appInfo);
        _path.Exists(Arg.Is<string>(x => x.EndsWith("Image.png"))).Returns(false);
        _path.Exists(@"C:\InstalledLocation\Package_v2\Image.png").Returns(true);

        // Act
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger,
            adrenalineGameId: gameInfo.Id);

        // Assert
        Assert.Equal("Edit Game", viewModel.Title);
        Assert.Equal(gameInfo.Id, viewModel.Id);
        Assert.Equal(gameInfo.DisplayName, viewModel.DisplayName);
        Assert.Equal(gameInfo.CommandLine, viewModel.Command);
        Assert.Equal(gameInfo.ImagePath, viewModel.ImagePath);
        Assert.Equal(gameInfo.ExePath, viewModel.ExePath);
        Assert.Equal(expectedOptions, viewModel.ExePathOptions);
        Assert.Equal(GameEditViewModel.CustomExePathOption, viewModel.SelectedExePathOption);
        Assert.True(viewModel.CanAutoFix);
        Assert.True(viewModel.ImagePathRequiresAttention);
        Assert.False(viewModel.ExePathRequiresAttention);
    }

    [Fact]
    public void Ctor_IsPackagedAppWithBrokenImagePathButNoAutoFixCandidate_CannotAutoFixImagePath()
    {
        // Arrange
        var appInfo = new PackagedAppInfo("Test Game",
            "Package!App",
            @"C:\InstalledLocation\Package_v2",
            DateTimeOffset.MinValue,
            null,
            @"Assets\Logo.png",
            true,
            "Game.exe",
            ["gamelauncherhelper.exe", "Game.exe"]);
        var expectedOptions = appInfo.ExecutablePaths
            .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
            .Concat([GameEditViewModel.CustomExePathOption]);
        _packagedAppsService.GetInfoAsync(appInfo.AppUserModelId).Returns(appInfo);
        var gameInfo = new AdrenalineGameInfo(Guid.Parse("9721c8ae-b162-4e64-bbc6-eac6d933b711"),
            "Test Display Name",
            @"C:\InstalledLocation\Package_v1\Image.png",
            appInfo.AppUserModelId,
            @"C:\InstalledLocation\Package_v1\Game.exe",
            true,
            true);
        _adrenalineGamesDataService.GamesData.Returns([gameInfo]);
        _packagedAppsService.GetInfoAsync(gameInfo.CommandLine).Returns(appInfo);
        _path.Exists(Arg.Is<string>(x => x.EndsWith("Image.png"))).Returns(false);

        // Act
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger,
            adrenalineGameId: gameInfo.Id);

        // Assert
        Assert.Equal("Edit Game", viewModel.Title);
        Assert.Equal(gameInfo.Id, viewModel.Id);
        Assert.Equal(gameInfo.DisplayName, viewModel.DisplayName);
        Assert.Equal(gameInfo.CommandLine, viewModel.Command);
        Assert.Equal(gameInfo.ImagePath, viewModel.ImagePath);
        Assert.Equal(gameInfo.ExePath, viewModel.ExePath);
        Assert.Equal(expectedOptions, viewModel.ExePathOptions);
        Assert.Equal(GameEditViewModel.CustomExePathOption, viewModel.SelectedExePathOption);
        Assert.False(viewModel.CanAutoFix);
        Assert.True(viewModel.ImagePathRequiresAttention);
        Assert.False(viewModel.ExePathRequiresAttention);
    }

    [Fact]
    public void Ctor_IsPackagedAppWithBrokenExePathAndAutoFixCandidate_CanAutoFixExePath()
    {
        // Arrange
        var appInfo = new PackagedAppInfo("Test Game",
            "Package!App",
            @"C:\InstalledLocation\Package_v2",
            DateTimeOffset.MinValue,
            null,
            @"Assets\Logo.png",
            true,
            "Game.exe",
            ["gamelauncherhelper.exe", "Game.exe"]);
        var expectedOptions = appInfo.ExecutablePaths
            .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
            .Concat([GameEditViewModel.CustomExePathOption]);
        _packagedAppsService.GetInfoAsync(appInfo.AppUserModelId).Returns(appInfo);
        var gameInfo = new AdrenalineGameInfo(Guid.Parse("9721c8ae-b162-4e64-bbc6-eac6d933b711"),
            "Test Display Name",
            @"C:\InstalledLocation\Package_v1\Image.png",
            appInfo.AppUserModelId,
            @"C:\InstalledLocation\Package_v1\Game.exe",
            true,
            true);
        _adrenalineGamesDataService.GamesData.Returns([gameInfo]);
        _adrenalineGamesDataService.GamesData.Returns([gameInfo]);
        _packagedAppsService.GetInfoAsync(gameInfo.CommandLine).Returns(appInfo);
        _path.Exists(Arg.Is<string>(x => x.EndsWith("Game.exe"))).Returns(false);
        _path.Exists(@"C:\InstalledLocation\Package_v2\Game.exe").Returns(true);

        // Act
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger,
            adrenalineGameId: gameInfo.Id);

        // Assert
        Assert.Equal("Edit Game", viewModel.Title);
        Assert.Equal(gameInfo.Id, viewModel.Id);
        Assert.Equal(gameInfo.DisplayName, viewModel.DisplayName);
        Assert.Equal(gameInfo.CommandLine, viewModel.Command);
        Assert.Equal(gameInfo.ImagePath, viewModel.ImagePath);
        Assert.Equal(gameInfo.ExePath, viewModel.ExePath);
        Assert.Equal(expectedOptions, viewModel.ExePathOptions);
        Assert.Equal(GameEditViewModel.CustomExePathOption, viewModel.SelectedExePathOption);
        Assert.True(viewModel.CanAutoFix);
        Assert.False(viewModel.ImagePathRequiresAttention);
        Assert.True(viewModel.ExePathRequiresAttention);
    }

    [Fact]
    public void Ctor_IsPackagedAppWithBrokenExePathButNoAutoFixCandidate_CannotAutoFixExePath()
    {
        // Arrange
        var appInfo = new PackagedAppInfo("Test Game",
            "Package!App",
            @"C:\InstalledLocation\Package_v2",
            DateTimeOffset.MinValue,
            null,
            @"Assets\Logo.png",
            true,
            "Game.exe",
            ["gamelauncherhelper.exe", "Game.exe"]);
        var expectedOptions = appInfo.ExecutablePaths
            .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
            .Concat([GameEditViewModel.CustomExePathOption]);
        _packagedAppsService.GetInfoAsync(appInfo.AppUserModelId).Returns(appInfo);
        var gameInfo = new AdrenalineGameInfo(Guid.Parse("9721c8ae-b162-4e64-bbc6-eac6d933b711"),
            "Test Display Name",
            @"C:\InstalledLocation\Package_v1\Image.png",
            appInfo.AppUserModelId,
            @"C:\InstalledLocation\Package_v1\Game.exe",
            true,
            true);
        _adrenalineGamesDataService.GamesData.Returns([gameInfo]);
        _adrenalineGamesDataService.GamesData.Returns([gameInfo]);
        _packagedAppsService.GetInfoAsync(gameInfo.CommandLine).Returns(appInfo);
        _path.Exists(Arg.Is<string>(x => x.EndsWith("Game.exe"))).Returns(false);

        // Act
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger,
            adrenalineGameId: gameInfo.Id);

        // Assert
        Assert.Equal("Edit Game", viewModel.Title);
        Assert.Equal(gameInfo.Id, viewModel.Id);
        Assert.Equal(gameInfo.DisplayName, viewModel.DisplayName);
        Assert.Equal(gameInfo.CommandLine, viewModel.Command);
        Assert.Equal(gameInfo.ImagePath, viewModel.ImagePath);
        Assert.Equal(gameInfo.ExePath, viewModel.ExePath);
        Assert.Equal(expectedOptions, viewModel.ExePathOptions);
        Assert.Equal(GameEditViewModel.CustomExePathOption, viewModel.SelectedExePathOption);
        Assert.False(viewModel.CanAutoFix);
        Assert.False(viewModel.ImagePathRequiresAttention);
        Assert.True(viewModel.ExePathRequiresAttention);
    }

    [InlineData(null, null, null)]
    [InlineData(@"C:\Path\Image.png", null, @"C:\Path\Image.png")]
    [InlineData(null, @"C:\Path\Game.exe", @"C:\Path\Game.exe")]
    [InlineData("", "", "")]
    [InlineData(@"C:\Path\Image.png", "", @"C:\Path\Image.png")]
    [InlineData("", @"C:\Path\Game.exe", @"C:\Path\Game.exe")]
    [InlineData(@"C:\Path\Image.png", @"C:\Path\Game.exe", @"C:\Path\Image.png")]
    [Theory]
    public void GetEffectiveImagePath_ReturnsImagePathWithExePathAsFallback(string? imagePath, string? exePath, string? expected)
    {
        _viewModel.ImagePath = imagePath;
        _viewModel.ExePath = exePath;

        var actual = _viewModel.EffectiveImagePath;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SetSelectedExePathOption_Null_SetsExePathToNull()
    {
        // Arrange
        var appInfo = new PackagedAppInfo("Test Game",
            "Package!App",
            @"C:\InstalledLocation",
            DateTimeOffset.MinValue,
            null,
            @"Assets\Logo.png",
            true,
            "Game.exe",
            ["Game.exe"]);
        _packagedAppsService.GetInfoAsync(appInfo.AppUserModelId).Returns(appInfo);
        _qualifiedFileResolver.GetPath(appInfo.InstalledPath, appInfo.Square150x150Logo!)
            .Returns(Path.Combine(appInfo.InstalledPath, appInfo.Square150x150Logo!));

        // Act
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger,
            appInfo.AppUserModelId)
        {
            SelectedExePathOption = null
        };

        // Assert
        Assert.Null(viewModel.ExePath);
    }

    [Fact]
    public async Task SaveAsync_WithData_CallsServiceWithData()
    {
        // Arrange
        _viewModel.Id = Guid.NewGuid();
        _viewModel.DisplayName = "Test Display Name";
        _viewModel.ImagePath = @"C:\Path\Image.png";
        _viewModel.Command = "Test-Command";
        _viewModel.ExePath = @"C:\Path\Executable.exe";
        _viewModel.IsManual = true;
        _viewModel.IsHidden = true;
        var expected = new AdrenalineGameInfo(_viewModel.Id,
            _viewModel.DisplayName,
            _viewModel.ImagePath,
            _viewModel.Command,
            _viewModel.ExePath,
            _viewModel.IsManual,
            _viewModel.IsHidden);

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        await _adrenalineGamesDataService.Received(1).SaveAsync(expected);
        _messenger.Received(1).Send(NavigateBackMessage.Instance, Arg.Any<TestMessengerToken>());
    }

    [Fact]
    public async Task SaveAsync_WithoutData_CallsServiceWithDefaults()
    {
        var expected = new AdrenalineGameInfo(_viewModel.Id,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            false,
            false);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        await _adrenalineGamesDataService.Received(1).SaveAsync(expected);
        _messenger.Received(1).Send(NavigateBackMessage.Instance, Arg.Any<TestMessengerToken>());
    }

    [Fact]
    public async Task SaveAsync_ServiceThrowsException_LogsError()
    {
        var gameInfo = new AdrenalineGameInfo(_viewModel.Id,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            false,
            false);
        _adrenalineGamesDataService.SaveAsync(Arg.Any<AdrenalineGameInfo>()).ThrowsAsync<Exception>();

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal($"Something went wrong while saving a game entry. {gameInfo}", _logger.LatestRecord.Message);
        Assert.Equal(LogLevel.Error, _logger.LatestRecord.Level);
    }

    [Fact]
    public async Task EditImageAsync_PickerReturnsNull_DoesNothing()
    {
        _filePicker.PickSingleFileAsync(Arg.Any<PickerLocationId>(), Arg.Any<IEnumerable<string>>())
            .Returns((string?)null);

        await _viewModel.EditImageCommand.ExecuteAsync(null);

        Assert.Null(_viewModel.ImagePath);
    }

    [Fact]
    public async Task EditImageAsync_PickerReturnsPath_DoesNothing()
    {
        var expected = @"C:\Path\Image.png";
        _filePicker.PickSingleFileAsync(Arg.Any<PickerLocationId>(), Arg.Any<IEnumerable<string>>())
            .Returns(expected);

        await _viewModel.EditImageCommand.ExecuteAsync(null);

        Assert.Equal(expected, _viewModel.ImagePath);
    }

    [Fact]
    public void RemoveImage_RemovesImagePath()
    {
        _viewModel.ImagePath = @"C:\Path\Image.png";

        _viewModel.RemoveImageCommand.Execute(null);

        Assert.Null(_viewModel.ImagePath);
    }

    [Fact]
    public void CopyImagePath_SetsImagePathInClipboard()
    {
        _viewModel.ImagePath = @"C:\Path\Image.png";

        _viewModel.CopyImagePathCommand.Execute(null);

        _clipboardService.Received(1).SetText(_viewModel.ImagePath);
    }

    [Fact]
    public void AutoFix_NoAutoFixCandidates_DoesNothing()
    {
        var expectedImagePath = @"C:\Path\Image.png";
        var expectedExePath = @"C:\Path\Game.exe";
        _viewModel.ImagePath = expectedImagePath;
        _viewModel.ExePath = expectedExePath;

        _viewModel.AutoFixCommand.Execute(null);

        Assert.Equal(expectedImagePath, _viewModel.ImagePath);
        Assert.Equal(expectedExePath, _viewModel.ExePath);
        Assert.False(_viewModel.CanAutoFix);
    }

    [Fact]
    public void AutoFix_HasAutoFixCandidates_UpdatesPaths()
    {
        // Arrange
        var expectedImagePath = @"C:\InstalledLocation\Package_v2\Image.png";
        var expectedExePath = @"C:\InstalledLocation\Package_v2\Game.exe";
        var appInfo = new PackagedAppInfo("Test Game",
            "Package!App",
            @"C:\InstalledLocation\Package_v2",
            DateTimeOffset.MinValue,
            null,
            @"Assets\Logo.png",
            true,
            "Game.exe",
            ["gamelauncherhelper.exe", "Game.exe"]);
        var expectedOptions = appInfo.ExecutablePaths
            .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
            .Concat([GameEditViewModel.CustomExePathOption]);
        _packagedAppsService.GetInfoAsync(appInfo.AppUserModelId).Returns(appInfo);
        var gameInfo = new AdrenalineGameInfo(Guid.Parse("9721c8ae-b162-4e64-bbc6-eac6d933b711"),
            "Test Display Name",
            @"C:\InstalledLocation\Package_v1\Image.png",
            appInfo.AppUserModelId,
            @"C:\InstalledLocation\Package_v1\Game.exe",
            true,
            true);
        _adrenalineGamesDataService.GamesData.Returns([gameInfo]);
        _packagedAppsService.GetInfoAsync(gameInfo.CommandLine).Returns(appInfo);
        _path.Exists(Arg.Is<string>(x => x.EndsWith("Image.png") || x.EndsWith("Game.exe"))).Returns(false);
        _path.Exists(expectedImagePath).Returns(true);
        _path.Exists(expectedExePath).Returns(true);
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _filePicker,
            _clipboardService,
            _dispatcherQueue,
            _path,
            _logger,
            adrenalineGameId: gameInfo.Id);

        // Act
        viewModel.AutoFixCommand.Execute(null);

        // Assert
        Assert.Equal(expectedImagePath, viewModel.ImagePath);
        Assert.Equal(expectedExePath, viewModel.ExePath);
        Assert.False(_viewModel.CanAutoFix);
    }
}
