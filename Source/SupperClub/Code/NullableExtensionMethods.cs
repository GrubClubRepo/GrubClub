using System;


public static class NullableExtensionMethods
{


    ///
    /// If null return an empty string otherwise returns the value formated according to the format string;
    ///
    public static string ToString(this DateTime? value, string format)
    {
        return value.HasValue ? value.Value.ToString(format) : string.Empty;
    }

    ///
    /// If null return an empty string otherwise returns the value formated according to the format string;
    ///
    public static string ToString(this decimal? value, string format)
    {
        return value.HasValue ? value.Value.ToString(format) : string.Empty;
    }
}
