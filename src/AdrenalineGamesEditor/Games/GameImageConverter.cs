using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

[ExcludeFromCodeCoverage(Justification = ExcludeFromCoverageJustifications.RequiresUIThread)]
internal class GameImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not GameListItemViewModel game)
            return null;

        if (!string.IsNullOrEmpty(game.ImagePath))
            return new BitmapImage(new Uri(game.ImagePath));

        using var icon = GetExeIcon(game.ExePath);
        return CreateBitmapImage(icon);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();

    private static Icon? GetExeIcon(string path)
    {
        Icon? icon = null;

        try
        {
            icon = Icon.ExtractIcon(path, 0, 96);
        }
        catch (IOException)
        { }

        if (icon is not null)
            return icon;

        try
        {
            icon = Icon.ExtractAssociatedIcon(path);
        }
        catch (IOException)
        { }

        return icon;
    }

    private static BitmapImage? CreateBitmapImage(Icon? icon)
    {
        if (icon is null)
            return null;

        using var iconBitmap = icon.ToBitmap();
        using var memoryStream = new MemoryStream();

        iconBitmap.Save(memoryStream, ImageFormat.Jpeg);
        memoryStream.Position = 0;

        var image = new BitmapImage();
        image.SetSource(memoryStream.AsRandomAccessStream());

        return image;
    }
}
