using System;
using System.Globalization;
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

        public static int ToDateInteger(this DateTime @this, string format = "yyyyMMdd") 
            => @this.ToString(format)
                .ToIntegerOrDefault();


        public static int ToInteger(this bool @this)
            => @this ? 1 : 0;

        public static int ToInteger(this bool? @this)
            => @this?.ToInteger() ?? 0;

        public static int ToIntegerOrDefault(this string @this)
        {
            if (string.IsNullOrWhiteSpace(@this))
                return default(int);

            if (int.TryParse(@this, out var integer))
                return integer;

            return default(int);
        }                       

        public static DateTime ToDateTimeOrDefault(this string @this, string format)
        {
            if (string.IsNullOrWhiteSpace(@this))
                return default(DateTime);

            if (DateTime.TryParseExact(@this, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))            
                return fecha;

            return default(DateTime);
        }
    }
}
