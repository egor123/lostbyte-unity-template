using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lostbyte.Toolkit.FactSystem
{
    internal static class FactUtils
    {
        public static string MakeSafeIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Unnamed";

            var sb = new StringBuilder();
            bool capitalizeNext = true;

            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(capitalizeNext ? char.ToUpperInvariant(c) : c);
                    capitalizeNext = false;
                }
                else
                {
                    capitalizeNext = true;
                }
            }
            if (sb.Length == 0 || !char.IsLetter(sb[0]))
            {
                sb.Insert(0, "F");
            }
            return sb.ToString();
        }
        public static string GenerateValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "default_name";
            name = Regex.Replace(name, @"([a-z0-9])([A-Z])", "$1_$2");
            name = Regex.Replace(name, @"[\s\-]+", "_");
            name = name.ToLowerInvariant();
            name = new string(name
                .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
                .ToArray());
            if (name.Length == 0 || !char.IsLetter(name[0]))
                name = "f" + name;
            if (!char.IsLetterOrDigit(name[^1]))
                name += "0";
            if (name.Length > 64)
                name = name[..64];
            var usedNames = FactDatabase.Instance.RootKeys
                .SelectMany(k => k.Children)
                .SelectMany(k => k.Children.Select(c => c.name).Concat(k.Facts.Select(f => f.name)))
                .ToHashSet();
            string result = name;
            int suffix = 1;
            while (usedNames.Contains(result))
                result = name + "_" + suffix++;
            return result;
        }
    }
}