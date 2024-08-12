using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.ApplicationModel.Resources;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MrCapitalQ.AdrenalineGamesEditor.Apps;

[ExcludeFromCodeCoverage(Justification = ExcludeFromCoverageJustifications.RequiresUIThread)]
public sealed class PackagedAppIcon : Control
{
    public static readonly DependencyProperty PackageProperty =
       DependencyProperty.Register(nameof(Package),
          typeof(IPackagedAppIconInfo),
          typeof(PackagedAppIcon),
          new PropertyMetadata(null));
    public static readonly DependencyProperty IconProperty =
       DependencyProperty.Register(nameof(Icon),
          typeof(ImageSource),
          typeof(PackagedAppIcon),
          new PropertyMetadata(null));

    private readonly QualifiedFileResolver _qualifiedFileResolver = new();
    private readonly Dictionary<string, string> _qualifiers = [];

    public PackagedAppIcon()
    {
        DefaultStyleKey = typeof(PackagedAppIcon);
        ActualThemeChanged += PackagedAppIcon_ActualThemeChanged;
        SizeChanged += PackagedAppIcon_SizeChanged;
    }

    public IPackagedAppIconInfo? Package
    {
        get => GetValue(PackageProperty) as IPackagedAppIconInfo;
        set
        {
            SetValue(PackageProperty, value);
            UpdateIconPath();
        }
    }

    public ImageSource? Icon
    {
        get => GetValue(IconProperty) as ImageSource;
        private set => SetValue(IconProperty, value);
    }

    private void UpdateIconPath()
    {
        if (Package is null || string.IsNullOrEmpty(Package.Square44x44Logo))
        {
            Icon = null;
            return;
        }

        var scale = (XamlRoot?.RasterizationScale ?? 1) * 100;
        var isLightMode = RequestedTheme == ElementTheme.Light
            || RequestedTheme == ElementTheme.Default && App.Current.RequestedTheme == ApplicationTheme.Light;
        var targetSize = Math.Min(ActualWidth, ActualHeight);

        _qualifiers[KnownResourceQualifierName.Scale] = scale.ToString(CultureInfo.InvariantCulture);
        _qualifiers[KnownResourceQualifierName.Theme] = isLightMode ? "light" : "dark";
        _qualifiers[KnownResourceQualifierName.TargetSize] = targetSize.ToString(CultureInfo.InvariantCulture);
        _qualifiers["AlternateForm"] = isLightMode ? "lightunplated" : "unplated";

        var path = _qualifiedFileResolver.GetPath(Package.InstalledPath, Package.Square44x44Logo, _qualifiers);
        Icon = new BitmapImage(new Uri(path));
    }

    private void PackagedAppIcon_ActualThemeChanged(FrameworkElement sender, object args) => UpdateIconPath();

    private void PackagedAppIcon_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateIconPath();
}
