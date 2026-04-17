using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Lostbyte.Toolkit.Localization
{
    public static class Formatter
    {
        private static readonly IFormatProvider _formatProvider = new PluralFormatProvider();
        public class PluralFormatProvider : IFormatProvider, ICustomFormatter
        {
            public object GetFormat(Type formatType)
            {
                return this;
            }


            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (format == null) return arg.ToString();
                if (!format.Contains('|'))
                {
                    if (arg is IFormattable f) return f.ToString(format, CultureInfo.CurrentCulture);
                    return null;
                }
                string[] forms = format.Split('|');
                int form = 0;
                if (arg is int iVal)
                {
                    form = Mathf.Abs(iVal) == 1 ? 0 : 1;
                }
                if (arg is string sVal)
                {
                    form = sVal.ToLower() switch
                    {
                        "m" => 0,
                        "male" => 0,
                        "female" => 1,
                        "f" => 1,
                        _ => forms.Length > 2 ? 2 : 0
                    };
                }
                return forms[Mathf.Clamp(form, 0, forms.Length)];
            }

        }
        public static string AdvFormat(string text, params Tuple<string, object>[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                (var key, var val) = args[i];
                text = text.Replace('{' + key, "{" + i);
            }
            return string.Format(_formatProvider, text, args.Select(a => a.Item2).ToArray());
        }
        public static string Format(string text, params object[] args)
        {
            return string.Format(_formatProvider, text, args);
        }
    }
}
