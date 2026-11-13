using System;
using System.Collections.Generic;

namespace NumberConversions.Core
{
    /// <summary>
    /// Handles Tamil Sandhi (புணர்ச்சி / வல்லொற்று மிகல் & நீக்கல்) at number boundaries:
    /// 1. Preceding Sandhi Removal: Removes obsolete doubled stops (க், ச், த், ப்) from preceding words 
    ///    (e.g. ஏறக்குறையப், கிட்டத்தட்டப், அந்தப், இந்தப், ஆகப்) when the following number word is converted to numeric digits.
    /// 2. Preceding Sandhi Addition: Adds required stop (e.g. 'ப்') to preceding trigger words when a number 
    ///    is converted to a word starting with 'ப' (10-19: பத்து, பன்னிரண்டு, etc.).
    /// 3. Following Sandhi Adjustment: Adjusts following doubled stop after numbers ending in வன்தொடர்க் குற்றியலுகரம் (பத்து, எட்டு).
    /// </summary>
    public static class TamilSandhiEngine
    {
        // High-frequency words before numbers that take sandhi before வல்லினம் (க, ச, த, ப)
        private static readonly HashSet<string> SandhiPrecedingTriggers = new HashSet<string>(StringComparer.Ordinal)
        {
            "ஏறக்குறைய",
            "கிட்டத்தட்ட",
            "அந்த",
            "இந்த",
            "எந்த",
            "ஆக",
            "முதலாக",
            "மொத்தமாக",
            "மிக",
            "முழுக்க"
        };

        private static readonly string[] SandhiStops = new[] { "ப்", "த்", "ச்", "க்" };

        /// <summary>
        /// Checks if the word immediately preceding the number at numberStartIndex ends in an obsolete 
        /// sandhi consonant (க், ச், த், ப்) that was doubled before a number word, and should be removed before digits.
        /// </summary>
        public static bool TryAdjustPrecedingSandhiForDigits(
            string text,
            int numberStartIndex,
            int numberLength,
            string formattedDigits,
            out int newStartIndex,
            out int newLength,
            out string newMatchedToken,
            out string newReplacement,
            out string contextMessage)
        {
            newStartIndex = numberStartIndex;
            newLength = numberLength;
            newMatchedToken = string.Empty;
            newReplacement = formattedDigits;
            contextMessage = string.Empty;

            if (numberStartIndex <= 0 || string.IsNullOrEmpty(text))
                return false;

            // Scan backwards from numberStartIndex
            int i = numberStartIndex - 1;
            while (i >= 0 && char.IsWhiteSpace(text[i]))
            {
                i--;
            }

            if (i < 0)
                return false;

            int wordEnd = i;
            while (i >= 0 && !char.IsWhiteSpace(text[i]) && !char.IsPunctuation(text[i]))
            {
                i--;
            }
            int wordStart = i + 1;

            if (wordEnd < wordStart)
                return false;

            string precedingWord = text.Substring(wordStart, wordEnd - wordStart + 1);

            // Check if precedingWord ends with a doubled stop: க், ச், த், or ப்
            foreach (var stop in SandhiStops)
            {
                if (precedingWord.EndsWith(stop))
                {
                    string baseWord = precedingWord.Substring(0, precedingWord.Length - stop.Length);
                    // In Tamil grammar, no native word ends with an isolated stop (க், ச், த், ப்).
                    // So if it ends with one of these before a number, it was a sandhi doubling.
                    if (baseWord.Length >= 2)
                    {
                        newStartIndex = wordStart;
                        int fullSpanEnd = numberStartIndex + numberLength;
                        newLength = fullSpanEnd - wordStart;
                        newMatchedToken = text.Substring(wordStart, newLength);

                        string whitespaceBetween = text.Substring(wordEnd + 1, numberStartIndex - (wordEnd + 1));
                        newReplacement = baseWord + whitespaceBetween + formattedDigits;
                        contextMessage = $"Sandhi: Removed obsolete trailing '{stop}' from '{precedingWord}' before digits";
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a preceding trigger word needs a sandhi consonant (e.g. 'ப்') added when a digit
        /// is converted to a Tamil word starting with 'ப' (10 to 19: பத்து, பன்னிரண்டு, etc.).
        /// </summary>
        public static bool TryAdjustPrecedingSandhiForWords(
            string text,
            int numberStartIndex,
            int numberLength,
            string tamilReplacementWord,
            out int newStartIndex,
            out int newLength,
            out string newMatchedToken,
            out string newReplacement,
            out string contextMessage)
        {
            newStartIndex = numberStartIndex;
            newLength = numberLength;
            newMatchedToken = string.Empty;
            newReplacement = tamilReplacementWord;
            contextMessage = string.Empty;

            if (numberStartIndex <= 0 || string.IsNullOrEmpty(text) || string.IsNullOrEmpty(tamilReplacementWord))
                return false;

            // Sandhi doubling before numbers only applies if the new word begins with 'ப' (வல்லினம்)
            if (!tamilReplacementWord.StartsWith("ப"))
                return false;

            int i = numberStartIndex - 1;
            while (i >= 0 && char.IsWhiteSpace(text[i]))
            {
                i--;
            }

            if (i < 0)
                return false;

            int wordEnd = i;
            while (i >= 0 && !char.IsWhiteSpace(text[i]) && !char.IsPunctuation(text[i]))
            {
                i--;
            }
            int wordStart = i + 1;

            if (wordEnd < wordStart)
                return false;

            string precedingWord = text.Substring(wordStart, wordEnd - wordStart + 1);

            // If the word is already ending with 'ப்', no need to add another
            if (precedingWord.EndsWith("ப்"))
                return false;

            if (SandhiPrecedingTriggers.Contains(precedingWord))
            {
                newStartIndex = wordStart;
                int fullSpanEnd = numberStartIndex + numberLength;
                newLength = fullSpanEnd - wordStart;
                newMatchedToken = text.Substring(wordStart, newLength);

                string whitespaceBetween = text.Substring(wordEnd + 1, numberStartIndex - (wordEnd + 1));
                newReplacement = precedingWord + "ப்" + whitespaceBetween + tamilReplacementWord;
                contextMessage = $"Sandhi: Added 'ப்' to '{precedingWord}' before '{tamilReplacementWord}'";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if following noun takes a sandhi consonant after numbers ending in வன்தொடர்க் குற்றியலுகரம் (பத்து, எட்டு).
        /// </summary>
        public static bool TryAdjustFollowingSandhiForWords(
            string text,
            int numberEndIndex,
            string tamilWord,
            out string adjustedTamilWord,
            out string contextMessage)
        {
            adjustedTamilWord = tamilWord;
            contextMessage = string.Empty;

            // Only வன்தொடர்க் குற்றியலுகரம் numbers (பத்து, எட்டு) double following stop
            if (tamilWord != "பத்து" && tamilWord != "எட்டு")
                return false;

            if (numberEndIndex >= text.Length)
                return false;

            int i = numberEndIndex;
            while (i < text.Length && char.IsWhiteSpace(text[i]))
            {
                i++;
            }

            if (i >= text.Length)
                return false;

            char nextChar = text[i];
            string stop = string.Empty;
            if (nextChar == 'ப') stop = "ப்";
            else if (nextChar == 'க') stop = "க்";
            else if (nextChar == 'ச') stop = "ச்";
            else if (nextChar == 'த') stop = "த்";

            if (!string.IsNullOrEmpty(stop))
            {
                adjustedTamilWord = tamilWord + stop;
                contextMessage = $"Sandhi: Added '{stop}' after '{tamilWord}' before '{nextChar}'";
                return true;
            }

            return false;
        }
    }
}
