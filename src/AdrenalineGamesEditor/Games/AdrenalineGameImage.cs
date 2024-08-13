using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

[ExcludeFromCodeCoverage(Justification = ExcludeFromCoverageJustifications.RequiresUIThread)]
public sealed class AdrenalineGameImage : Control
{
    public static readonly DependencyProperty GameProperty =
       DependencyProperty.Register(nameof(Game),
          typeof(IAdrenalineGameImageInfo),
          typeof(AdrenalineGameImage),
          new PropertyMetadata(null));
    public static readonly DependencyProperty ImageProperty =
       DependencyProperty.Register(nameof(Image),
          typeof(ImageSource),
          typeof(AdrenalineGameImage),
          new PropertyMetadata(null));

    public AdrenalineGameImage()
    {
        DefaultStyleKey = typeof(AdrenalineGameImage);
        SizeChanged += AdrenalineGameImage_SizeChanged;
    }

    public IAdrenalineGameImageInfo? Game
    {
        get => GetValue(GameProperty) as IAdrenalineGameImageInfo;
        set
        {
            SetValue(GameProperty, value);
            UpdateImage();
        }
    }

    public ImageSource? Image
    {
        get => GetValue(ImageProperty) as ImageSource;
        private set => SetValue(ImageProperty, value);
    }

    private void UpdateImage()
    {
        if (!string.IsNullOrEmpty(Game?.ImagePath))
        {
            Image = new BitmapImage(new Uri(Game.ImagePath))
            {
                DecodePixelWidth = (int)ActualWidth,
                DecodePixelHeight = (int)ActualHeight
            };
        }
        else if (!string.IsNullOrEmpty(Game?.ExePath))
        {
            using var icon = GetExeIcon(Game.ExePath);
            Image = CreateBitmapImage(icon);
        }
        else
            Image = null;
    }

    private Icon? GetExeIcon(string path)
    {
        Icon? icon = null;

        try
        {
            var size = (int)(Math.Min(ActualWidth, ActualHeight) * (XamlRoot?.RasterizationScale ?? 1));
            if (size <= 0)
                return null;

            icon = Icon.ExtractIcon(path, 0, size);
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

    private BitmapImage? CreateBitmapImage(Icon? icon)
    {
        if (icon is null)
            return null;

        using var iconBitmap = icon.ToBitmap();
        using var memoryStream = new MemoryStream();

        iconBitmap.Save(memoryStream, ImageFormat.Jpeg);
        memoryStream.Position = 0;

        var image = new BitmapImage
        {
            DecodePixelWidth = (int)ActualWidth,
            DecodePixelHeight = (int)ActualHeight
        };
        image.SetSource(memoryStream.AsRandomAccessStream());

        return image;
    }

    private void AdrenalineGameImage_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateImage();
}
