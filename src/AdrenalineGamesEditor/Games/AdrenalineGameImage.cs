using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

[ExcludeFromCodeCoverage(Justification = ExcludeFromCoverageJustifications.RequiresUIThread)]
public sealed class AdrenalineGameImage : Control
{
    public static readonly DependencyProperty SourcePathProperty =
       DependencyProperty.Register(nameof(SourcePath),
          typeof(string),
          typeof(AdrenalineGameImage),
          new PropertyMetadata(null));
    public static readonly DependencyProperty ImageProperty =
       DependencyProperty.Register(nameof(Image),
          typeof(BitmapImage),
          typeof(AdrenalineGameImage),
          new PropertyMetadata(null));

    public AdrenalineGameImage()
    {
        DefaultStyleKey = typeof(AdrenalineGameImage);
        SizeChanged += AdrenalineGameImage_SizeChanged;
    }

    public string? SourcePath
    {
        get => GetValue(SourcePathProperty) as string;
        set
        {
            if (SourcePath == value)
                return;

            SetValue(SourcePathProperty, value);
            UpdateImage();
        }
    }

    public BitmapImage? Image
    {
        get => GetValue(ImageProperty) as BitmapImage;
        private set => SetValue(ImageProperty, value);
    }

    private void UpdateImage()
    {
        if (string.IsNullOrEmpty(SourcePath))
        {
            Image = null;
        }
        else if (".exe".Equals(Path.GetExtension(SourcePath), StringComparison.OrdinalIgnoreCase))
        {
            using var icon = GetExeIcon(SourcePath);
            Image = CreateBitmapImage(icon);
        }
        else
        {
            var uriSource = new Uri(SourcePath);
            if (Image?.UriSource != uriSource)
                if (Image?.UriSource is not null)
                    Image.UriSource = uriSource;
                else
                    Image = new BitmapImage(uriSource);
        }

        if (Image is not null)
        {
            Image.DecodePixelWidth = (int)ActualWidth;
            Image.DecodePixelHeight = (int)ActualHeight;
        }
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

    private static BitmapImage? CreateBitmapImage(Icon? icon)
    {
        if (icon is null)
            return null;

        using var iconBitmap = icon.ToBitmap();
        using var memoryStream = new MemoryStream();

        iconBitmap.Save(memoryStream, ImageFormat.Png);
        memoryStream.Position = 0;

        var image = new BitmapImage();
        image.SetSource(memoryStream.AsRandomAccessStream());

        return image;
    }

    private void AdrenalineGameImage_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateImage();
}
