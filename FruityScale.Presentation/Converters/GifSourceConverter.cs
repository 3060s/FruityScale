using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Labs.Gif;

namespace FruityScale.Presentation.Converters;

// GifSourceConverter code was taken from:
// https://github.com/AvaloniaUI/Avalonia.Labs/blob/7d86b80d576b9c0052fda104303c1ec1acb74f53/samples/Avalonia.Labs.Catalog/Converters/GifSourceConverter.cs
public class GifSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        switch (value)
        {
            case Stream gifSourceStream:
                return GifStreamSource.FromStream(gifSourceStream);
            case string gifSourceUriString:
                return GifStreamSource.FromUriString(gifSourceUriString);
            case Uri gifSourceUri:
                return GifStreamSource.FromUri(gifSourceUri);
            default:
                return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}