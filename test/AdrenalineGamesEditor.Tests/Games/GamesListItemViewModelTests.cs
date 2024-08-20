using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using MrCapitalQ.AdrenalineGamesEditor.Games;

namespace MrCapitalQ.AdrenalineGamesEditor.Tests.Games;

public class GamesListItemViewModelTests
{
    [InlineData("", "", "")]
    [InlineData(@"C:\Path\Image.png", "", @"C:\Path\Image.png")]
    [InlineData("", @"C:\Path\Game.exe", @"C:\Path\Game.exe")]
    [InlineData(@"C:\Path\Image.png", @"C:\Path\Game.exe", @"C:\Path\Image.png")]
    [Theory]
    public void CreateFromInfo_SetsPropertiesFromInfo(string imagePath,
        string exePath,
        string expectedEffectiveImagePath)
    {
        var gameInfo = new AdrenalineGameInfo(Guid.Parse("9721c8ae-b162-4e64-bbc6-eac6d933b711"),
            "Test Display Name",
            imagePath,
            "Test-Command",
            exePath,
            true,
            true);

        var actual = GamesListItemViewModel.CreateFromInfo(gameInfo);

        Assert.Equal(gameInfo.Id, actual.Id);
        Assert.Equal(gameInfo.DisplayName, actual.DisplayName);
        Assert.Equal(expectedEffectiveImagePath, actual.EffectiveImagePath);
        Assert.Equal(gameInfo.IsManual, actual.IsManual);
    }

    [InlineData("", "", "")]
    [InlineData(@"C:\Path\Image.png", "", @"C:\Path\Image.png")]
    [InlineData("", @"C:\Path\Game.exe", @"C:\Path\Game.exe")]
    [InlineData(@"C:\Path\Image.png", @"C:\Path\Game.exe", @"C:\Path\Image.png")]
    [Theory]
    public void UpdateFromInfo_SetsPropertiesFromInfo(string imagePath,
        string exePath,
        string expectedEffectiveImagePath)
    {
        var gameInfo = new AdrenalineGameInfo(Guid.Parse("9721c8ae-b162-4e64-bbc6-eac6d933b711"),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            false,
            true);
        var updatedGameInfo = new AdrenalineGameInfo(gameInfo.Id,
            "Test Display Name",
            imagePath,
            "Test-Command",
            exePath,
            true,
            true);
        var actual = GamesListItemViewModel.CreateFromInfo(gameInfo);

        actual.UpdateFromInfo(updatedGameInfo);

        Assert.Equal(updatedGameInfo.Id, actual.Id);
        Assert.Equal(updatedGameInfo.DisplayName, actual.DisplayName);
        Assert.Equal(expectedEffectiveImagePath, actual.EffectiveImagePath);
        Assert.Equal(updatedGameInfo.IsManual, actual.IsManual);
        Assert.Equal(updatedGameInfo.IsHidden, actual.IsHidden);
    }
}
