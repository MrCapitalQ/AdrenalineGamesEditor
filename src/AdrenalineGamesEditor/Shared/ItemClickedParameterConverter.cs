using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.AdrenalineGamesEditor.Shared;

[ExcludeFromCodeCoverage(Justification = "ItemClickEventArgs cannot be instantiated.")]
internal class ItemClickedParameterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is ItemClickEventArgs args)
            return args.ClickedItem;

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}
