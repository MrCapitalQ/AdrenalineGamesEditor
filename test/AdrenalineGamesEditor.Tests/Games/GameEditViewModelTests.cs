using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Games;
using MrCapitalQ.AdrenalineGamesEditor.Shared;
using NSubstitute.ExceptionExtensions;

namespace MrCapitalQ.AdrenalineGamesEditor.Tests.Games;

public class GameEditViewModelTests
{
    private readonly IPackagedAppsService _packagedAppsService = Substitute.For<IPackagedAppsService>();
    private readonly IQualifiedFileResolver _qualifiedFileResolver = Substitute.For<IQualifiedFileResolver>();
    private readonly IAdrenalineGamesDataService _adrenalineGamesDataService = Substitute.For<IAdrenalineGamesDataService>();
    private readonly IMessenger _messenger = Substitute.For<IMessenger>();
    private readonly FakeLogger<GameEditViewModel> _logger = new();

    private readonly GameEditViewModel _viewModel;

    public GameEditViewModelTests()
    {
        _viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
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
            @"Game.exe",
            ["gamelauncherhelper.exe", @"Game.exe"]);
        _packagedAppsService.GetInfoAsync(appInfo.AppUserModelId).Returns(appInfo);
        _qualifiedFileResolver.GetPath(appInfo.InstalledPath, appInfo.Square150x150Logo!)
            .Returns(Path.Combine(appInfo.InstalledPath, appInfo.Square150x150Logo!));

        // Act
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _logger,
            appInfo.AppUserModelId);

        // Assert
        Assert.Equal(Guid.Empty, viewModel.Id);
        Assert.Equal(appInfo.DisplayName, viewModel.DisplayName);
        Assert.Equal(appInfo.AppUserModelId, viewModel.Command);
        Assert.Equal(new Uri(Path.Combine(appInfo.InstalledPath, appInfo.Square150x150Logo!)), viewModel.ImagePath);
        Assert.Equal(Path.Combine(appInfo.InstalledPath, appInfo.ExecutablePath!), viewModel.ExePath);
        Assert.Equal(appInfo.ExecutablePaths.Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}")), viewModel.ExePathOptions);
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
        _packagedAppsService.GetInfoAsync(appInfo.AppUserModelId).Returns(appInfo);
        _qualifiedFileResolver.GetPath(appInfo.InstalledPath, appInfo.Square150x150Logo!)
            .Returns(Path.Combine(appInfo.InstalledPath, appInfo.Square150x150Logo!));

        // Act
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _logger,
            appInfo.AppUserModelId);

        // Assert
        Assert.Equal(Guid.Empty, viewModel.Id);
        Assert.Equal(appInfo.DisplayName, viewModel.DisplayName);
        Assert.Equal(appInfo.AppUserModelId, viewModel.Command);
        Assert.Equal(new Uri(Path.Combine(appInfo.InstalledPath, appInfo.Square150x150Logo!)), viewModel.ImagePath);
        Assert.Null(viewModel.ExePath);
        Assert.Equal(appInfo.ExecutablePaths.Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}")), viewModel.ExePathOptions);
    }

    [Fact]
    public void Ctor_AppUserModelIdNotFound_DoesNotInitialize()
    {
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
            _logger,
            "Package!App");

        Assert.Equal(Guid.Empty, viewModel.Id);
        Assert.Null(viewModel.DisplayName);
        Assert.Null(viewModel.Command);
        Assert.Null(viewModel.ImagePath);
        Assert.Null(viewModel.ExePath);
        Assert.Empty(viewModel.ExePathOptions);
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
            @"Game.exe",
            [@"Game.exe"]);
        _packagedAppsService.GetInfoAsync(appInfo.AppUserModelId).Returns(appInfo);
        _qualifiedFileResolver.GetPath(appInfo.InstalledPath, appInfo.Square150x150Logo!)
            .Returns(Path.Combine(appInfo.InstalledPath, appInfo.Square150x150Logo!));

        // Act
        var viewModel = new GameEditViewModel(_packagedAppsService,
            _qualifiedFileResolver,
            _adrenalineGamesDataService,
            _messenger,
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
        _viewModel.ImagePath = new Uri(@"C:\Path\Image.png");
        _viewModel.Command = "Test-Command";
        _viewModel.ExePath = @"C:\Path\Executabe.exe";
        var expected = new AdrenalineGameInfo(_viewModel.Id,
            _viewModel.DisplayName,
            _viewModel.ImagePath.LocalPath,
            _viewModel.Command,
            _viewModel.ExePath,
            true);

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        await _adrenalineGamesDataService.Received(1).AddAsync(expected);
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
            true);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        await _adrenalineGamesDataService.Received(1).AddAsync(expected);
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
            true);
        _adrenalineGamesDataService.AddAsync(Arg.Any<AdrenalineGameInfo>()).ThrowsAsync<Exception>();

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal($"Something went wrong while saving a game entry. {gameInfo}", _logger.LatestRecord.Message);
        Assert.Equal(LogLevel.Error, _logger.LatestRecord.Level);
    }
}
