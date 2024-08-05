using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Games;
using MrCapitalQ.AdrenalineGamesEditor.Shared;

namespace MrCapitalQ.AdrenalineGamesEditor.Tests.Games;

public class GameEditViewModelTests
{
    private readonly IPackagedAppsService _packagedAppsService = Substitute.For<IPackagedAppsService>();
    private readonly IQualifiedFileResolver _qualifiedFileResolver = Substitute.For<IQualifiedFileResolver>();

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
        var viewModel = new GameEditViewModel(_packagedAppsService, _qualifiedFileResolver, appInfo.AppUserModelId);

        // Assert
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
        var viewModel = new GameEditViewModel(_packagedAppsService, _qualifiedFileResolver, appInfo.AppUserModelId);

        // Assert
        Assert.Equal(appInfo.DisplayName, viewModel.DisplayName);
        Assert.Equal(appInfo.AppUserModelId, viewModel.Command);
        Assert.Equal(new Uri(Path.Combine(appInfo.InstalledPath, appInfo.Square150x150Logo!)), viewModel.ImagePath);
        Assert.Null(viewModel.ExePath);
        Assert.Equal(appInfo.ExecutablePaths.Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}")), viewModel.ExePathOptions);
    }

    [Fact]
    public void Ctor_AppUserModelIdNotFound_DoesNotInitialize()
    {
        var viewModel = new GameEditViewModel(_packagedAppsService, _qualifiedFileResolver, "Package!App");

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
        var viewModel = new GameEditViewModel(_packagedAppsService, _qualifiedFileResolver, appInfo.AppUserModelId);

        // Act
        viewModel.SelectedExePathOption = null;

        // Assert
        Assert.Null(viewModel.ExePath);
    }
}
