using System;
using System.Text.RegularExpressions;

namespace NumberConversions.Core
{
    public enum OrdinalStyle
    {
        Aam,    // e.g. முதலாம், இரண்டாம், மூன்றாம்
        Aavathu // e.g. முதலாவது, இரண்டாவது, மூன்றாவது
    }

    /// <summary>
    /// Converts ordinals (e.g. 2nd, 2-ஆம், 2வது, 175-ஆம்) to Tamil text words.
    /// </summary>
    public static class TamilOrdinalToTextConverter
    {
        private static readonly Regex OrdinalPattern = new Regex(
            @"^(\d{1,3})(?:st|nd|rd|th|-?ஆம்|-?வது|ம்)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool TryMatchOrdinal(string text, out int number, out string matchedSuffix)
        {
            number = 0;
            matchedSuffix = string.Empty;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            var match = OrdinalPattern.Match(text.Trim());
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out number))
                {
                    matchedSuffix = text.Substring(match.Groups[1].Length);
                    return true;
                }
            }

            return false;
        }

        public static string Convert(int number, OrdinalStyle style = OrdinalStyle.Aam)
        {
            if (number < 0 || number > 199)
                throw new ArgumentOutOfRangeException(nameof(number), "Supported range is 0 to 199.");

            if (number == 1)
                return style == OrdinalStyle.Aavathu ? "முதலாவது" : "முதலாம்";

            if (number <= 10 && style == OrdinalStyle.Aam)
                return TamilNumberLexicon.UnitOrdinals[number];

            string cardinal = TamilCardinalToTextConverter.Convert(number);

            // Apply ordinal suffix to cardinal word:
            // Words ending in 'ு' (u) replace with 'ாம்' or 'ாவது':
            // e.g. இரண்டு -> இரண்டாம் / இரண்டாவது
            // e.g. ஐந்து -> ஐந்தாம் / ஐந்தாவது
            // e.g. பத்து -> பத்தாம் / பத்தாவது
            // e.g. நூறு -> நூறாம் / நூறாவது
            if (cardinal.EndsWith("ு"))
            {
                string baseWord = cardinal.Substring(0, cardinal.Length - 1);
                return style == OrdinalStyle.Aavathu ? baseWord + "ாவது" : baseWord + "ாம்";
            }
            else if (cardinal.EndsWith("து") || cardinal.EndsWith("று"))
            {
                string baseWord = cardinal.Substring(0, cardinal.Length - 1);
                return style == OrdinalStyle.Aavathu ? baseWord + "ாவது" : baseWord + "ாம்";
            }
            else
            {
                return style == OrdinalStyle.Aavathu ? cardinal + "ாவது" : cardinal + "ாம்";
            }
        }
    }
}
