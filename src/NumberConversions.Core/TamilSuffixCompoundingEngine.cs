using System;

namespace NumberConversions.Core
{
    /// <summary>
    /// Handles suffix compounding and postposition joining for Tamil numbers:
    /// 1. Ordinals: -ஆம், -ஆவது, -வது (e.g. 13 ஆம் -> பதின்மூன்றாம், 14 ஆம் -> பதினான்காம், 13 ஆவது -> பதின்மூன்றாவது)
    /// 2. Locative Case: -இல், -ல் (e.g. 13 இல் -> பதின்மூன்றில், 100 இல் -> நூற்றில்)
    /// 3. Ablative Case: -இலிருந்து, -லிருந்து (e.g. 13 இலிருந்து -> பதின்மூன்றிலிருந்து, 100 இலிருந்து -> நூற்றிலிருந்து)
    /// 4. Dative Case: -க்கு, -உக்கு (e.g. 13 க்கு -> பதின்மூன்றுக்கு, 100 க்கு -> நூற்றுக்கு)
    /// 5. Accusative Case: -ஐ (e.g. 13 ஐ -> பதின்மூன்றை, 100 ஐ -> நூறை)
    /// </summary>
    public static class TamilSuffixCompoundingEngine
    {
        public static bool TryConvertNumberWithSuffix(
            int number,
            string suffix,
            out string replacement)
        {
            replacement = string.Empty;
            if (number < 0 || number > 199 || string.IsNullOrEmpty(suffix))
                return false;

            string cleanSuffix = suffix.Trim().TrimStart('-');

            // Ordinal: ஆம் / ம் / st / nd / rd / th
            if (cleanSuffix == "ஆம்" || cleanSuffix == "ம்" ||
                cleanSuffix.Equals("st", StringComparison.OrdinalIgnoreCase) ||
                cleanSuffix.Equals("nd", StringComparison.OrdinalIgnoreCase) ||
                cleanSuffix.Equals("rd", StringComparison.OrdinalIgnoreCase) ||
                cleanSuffix.Equals("th", StringComparison.OrdinalIgnoreCase))
            {
                replacement = TamilOrdinalToTextConverter.Convert(number, OrdinalStyle.Aam);
                return true;
            }

            // Ordinal: ஆவது / வது
            if (cleanSuffix == "ஆவது" || cleanSuffix == "வது")
            {
                replacement = TamilOrdinalToTextConverter.Convert(number, OrdinalStyle.Aavathu);
                return true;
            }

            // Locative Case: இல் / ல்
            if (cleanSuffix == "இல்" || cleanSuffix == "ல்")
            {
                if (number == 100)
                {
                    replacement = "நூற்றில்";
                    return true;
                }
                string cardinal = TamilCardinalToTextConverter.Convert(number);
                if (cardinal.EndsWith("ு"))
                {
                    replacement = cardinal.Substring(0, cardinal.Length - 1) + "ில்";
                }
                else
                {
                    replacement = cardinal + "ில்";
                }
                return true;
            }

            // Ablative Case: இலிருந்து / லிருந்து
            if (cleanSuffix == "இலிருந்து" || cleanSuffix == "லிருந்து")
            {
                if (number == 100)
                {
                    replacement = "நூற்றிலிருந்து";
                    return true;
                }
                string cardinal = TamilCardinalToTextConverter.Convert(number);
                if (cardinal.EndsWith("ு"))
                {
                    replacement = cardinal.Substring(0, cardinal.Length - 1) + "ிலிருந்து";
                }
                else
                {
                    replacement = cardinal + "ிலிருந்து";
                }
                return true;
            }

            // Dative Case: க்கு / உக்கு
            if (cleanSuffix == "க்கு" || cleanSuffix == "உக்கு")
            {
                if (number == 100)
                {
                    replacement = "நூற்றுக்கு";
                    return true;
                }
                string cardinal = TamilCardinalToTextConverter.Convert(number);
                replacement = cardinal + "க்கு";
                return true;
            }

            // Accusative Case: ஐ
            if (cleanSuffix == "ஐ")
            {
                if (number == 100)
                {
                    replacement = "நூறை";
                    return true;
                }
                string cardinal = TamilCardinalToTextConverter.Convert(number);
                if (cardinal.EndsWith("ு"))
                {
                    replacement = cardinal.Substring(0, cardinal.Length - 1) + "ை";
                }
                else
                {
                    replacement = cardinal + "ை";
                }
                return true;
            }

            return false;
        }
    }
}
