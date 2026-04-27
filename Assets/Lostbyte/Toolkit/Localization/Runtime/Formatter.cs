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
                if (formatType == typeof(ICustomFormatter))
                    return this;

                return null;
            }

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (arg == null) return string.Empty;

                if (format != null && format.Contains('|'))
                {
                    string[] forms = format.Split('|');
                    int form = 0;

                    if (arg is int iVal)
                    {
                        form = Mathf.Abs(iVal) == 1 ? 0 : 1;
                    }
                    else if (arg is string sVal)
                    {
                        form = sVal.ToLower() switch
                        {
                            "m" or "male" => 0,
                            "f" or "female" => 1,
                            _ => forms.Length > 2 ? 2 : 0
                        };
                    }
                    return forms[Mathf.Clamp(form, 0, forms.Length - 1)];
                }

                if (arg is IFormattable formattable)
                    return formattable.ToString(format, CultureInfo.CurrentCulture);

                return arg.ToString();
            }
        }

        public static string AdvFormat(string text, params Tuple<string, object>[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                (var key, var val) = args[i];
                // Replace "{key" instead of "{key}" so it naturally handles "{key:format}" too
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