using System;
using System.Text.RegularExpressions;

namespace Sisfarma.Sincronizador.Extensions
{
    public static class UtilExtensions
    {
        public static string Strip(this string word) => word != null
                ? Regex.Replace(word.Trim(), @"[.',\-\\]", string.Empty)
                : string.Empty;

        public static string ToIsoString(this DateTime? @this)
            => @this.HasValue
                ? @this.Value.ToIsoString()
                : DateTime.MinValue.ToIsoString();

        public static string ToIsoString(this DateTime @this) 
            => @this.ToString("yyyy-MM-ddTHH:mm:ss");

        public static int ToInteger(this bool @this)
            => @this ? 1 : 0;
    }
}
