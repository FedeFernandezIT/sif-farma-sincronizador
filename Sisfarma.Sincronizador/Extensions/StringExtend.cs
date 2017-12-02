using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Extensions
{
    public static class StringExtend
    {
        public static string Strip(this string word) => word != null
                ? Regex.Replace(word.Trim(), @"[.',\-\\]", string.Empty)
                : string.Empty;
    }
}
