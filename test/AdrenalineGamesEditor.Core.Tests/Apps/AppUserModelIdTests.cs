﻿using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;

namespace MrCapitalQ.AdrenalineGamesEditor.Core.Tests.Apps;

public class AppUserModelIdTests
{
    [Fact]
    public void Parse_ValidId_ParsesOutFamilyNameAndAppId()
    {
        var expectedPackageFamilyName = "PackageFamilyName";
        var expectedPackageAppId = "App";
        var applicationUserModelId = $"{expectedPackageFamilyName}!{expectedPackageAppId}";

        var actual = AppUserModelId.Parse(applicationUserModelId);

        Assert.Equal(expectedPackageFamilyName, actual.PackageFamilyName);
        Assert.Equal(expectedPackageAppId, actual.PackageAppId);
        Assert.Equal(applicationUserModelId, actual.ValueAsString);
    }

    [InlineData("!")]
    [InlineData("MissingAppId!")]
    [InlineData("!MissingFamilyName")]
    [Theory]
    public void Parse_InvalidIdFormat_ThrowsFormatException(string applicationUserModelId)
    {
        Assert.Throws<FormatException>(() => AppUserModelId.Parse(applicationUserModelId));
    }

    [InlineData("")]
    [InlineData(" ")]
    [Theory]
    public void Parse_EmptyId_ThrowsArgumentException(string applicationUserModelId)
    {
        Assert.Throws<ArgumentException>(() => AppUserModelId.Parse(applicationUserModelId));
    }

    [Fact]
    public void TryParse_ValidId_ReturnsTrueAndParsesOutFamilyNameAndAppId()
    {
        var expectedPackageFamilyName = "PackageFamilyName";
        var expectedPackageAppId = "App";
        var applicationUserModelId = $"{expectedPackageFamilyName}!{expectedPackageAppId}";

        var actual = AppUserModelId.TryParse(applicationUserModelId, out var id);

        Assert.True(actual);
        Assert.NotNull(id);
        Assert.Equal(expectedPackageFamilyName, id.PackageFamilyName);
        Assert.Equal(expectedPackageAppId, id.PackageAppId);
        Assert.Equal(applicationUserModelId, id.ValueAsString);
    }

    [InlineData("!")]
    [InlineData("MissingAppId!")]
    [InlineData("!MissingFamilyName")]
    [InlineData("")]
    [InlineData(" ")]
    [Theory]
    public void TryParse_InvalidId_ReturnsFalse(string applicationUserModelId)
    {
        var actual = AppUserModelId.TryParse(applicationUserModelId, out var id);

        Assert.False(actual);
        Assert.Null(id);
    }
}
