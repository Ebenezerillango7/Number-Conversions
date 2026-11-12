using System;
using System.Collections.Generic;

namespace NumberConversions.Core
{
    /// <summary>
    /// Formats numbers using the South Asian / Indian numbering system (3, 2, 2... grouping).
    /// Example: 1,000 | 12,345 | 1,44,000 | 12,34,567 | 1,23,45,678
    /// </summary>
    public static class IndianNumberFormatter
    {
        public static string Format(long value)
        {
            if (value < 0)
                return "-" + Format(-value);

            if (value < 1000)
                return value.ToString();

            string raw = value.ToString();
            string lastThree = raw.Substring(raw.Length - 3);
            string remaining = raw.Substring(0, raw.Length - 3);

            var groups = new List<string>();
            while (remaining.Length > 2)
            {
                groups.Insert(0, remaining.Substring(remaining.Length - 2));
                remaining = remaining.Substring(0, remaining.Length - 2);
            }

            if (remaining.Length > 0)
            {
                groups.Insert(0, remaining);
            }

            return string.Join(",", groups) + "," + lastThree;
        }

        public static string Format(int value)
        {
            return Format((long)value);
        }

        public static bool TryParseIndianNumber(string input, out long result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            string clean = input.Replace(",", "").Trim();
            return long.TryParse(clean, out result);
        }
    }
}
