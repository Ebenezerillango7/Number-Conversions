using System;
using System.Collections.Generic;

namespace NumberConversions.Core
{
    /// <summary>
    /// Parses Tamil number phrases and words for values >= 200 and converts them to formatted digits.
    /// </summary>
    public static class TamilTextToNumberConverter
    {
        private static readonly Dictionary<string, long> DirectWordValues = new Dictionary<string, long>(StringComparer.Ordinal)
        {
            // Digits
            { "பூஜ்யம்", 0 }, { "பூச்சியம்", 0 },
            { "ஒன்று", 1 }, { "ஒரு", 1 }, { "ஓர்", 1 },
            { "இரண்டு", 2 }, { "இரு", 2 },
            { "மூன்று", 3 }, { "மு", 3 }, { "முப்ப", 3 },
            { "நான்கு", 4 }, { "நாலு", 4 }, { "நா", 4 },
            { "ஐந்து", 5 }, { "ஐ", 5 },
            { "ஆறு", 6 }, { "அறு", 6 },
            { "ஏழு", 7 }, { "எழு", 7 },
            { "எட்டு", 8 }, { "எண்", 8 },
            { "ஒன்பது", 9 }, { "ஒன்ப", 9 },

            // Teens
            { "பத்து", 10 }, { "பதின்", 10 }, { "பதின", 10 },
            { "பதினொன்று", 11 }, { "பன்னிரண்டு", 12 }, { "பதின்மூன்று", 13 },
            { "பதினான்கு", 14 }, { "பதினைந்து", 15 }, { "பதினாறு", 16 },
            { "பதினேழு", 17 }, { "பதினெட்டு", 18 }, { "பத்தொன்பது", 19 },

            // Tens
            { "இருபது", 20 }, { "இருபத்து", 20 },
            { "முப்பது", 30 }, { "முப்பத்து", 30 },
            { "நாற்பது", 40 }, { "நாற்பத்து", 40 },
            { "ஐம்பது", 50 }, { "ஐம்பத்து", 50 },
            { "அறுபது", 60 }, { "அறுபத்து", 60 },
            { "எழுபது", 70 }, { "எழுபத்து", 70 },
            { "எண்பது", 80 }, { "எண்பத்து", 80 },
            { "தொண்ணூறு", 90 }, { "தொண்ணூற்று", 90 },

            // Hundreds
            { "நூறு", 100 }, { "நூற்று", 100 }, { "நூற", 100 },
            { "இருநூறு", 200 }, { "இருநூற்று", 200 }, { "இருநூற", 200 },
            { "முந்நூறு", 300 }, { "முந்நூற்று", 300 }, { "முந்நூற", 300 },
            { "நானூறு", 400 }, { "நானூற்று", 400 }, { "நானூற", 400 },
            { "ஐந்நூறு", 500 }, { "ஐந்நூற்று", 500 }, { "ஐந்நூற", 500 },
            { "அறுநூறு", 600 }, { "அறுநூற்று", 600 }, { "அறுநூற", 600 },
            { "எழுநூறு", 700 }, { "எழுநூற்று", 700 }, { "எழுநூற", 700 },
            { "எண்ணூறு", 800 }, { "எண்ணூற்று", 800 }, { "எண்ணூற", 800 },
            { "தொள்ளாயிரம்", 900 }, { "தொள்ளாயிரத்து", 900 }, { "தொள்ளாயிரத்த", 900 },

            // Thousands
            { "ஆயிரம்", 1000 }, { "ஆயிரத்து", 1000 }, { "ஆயிரத்த", 1000 }, { "ஓராயிரம்", 1000 },
            { "இரண்டாயிரம்", 2000 }, { "இரண்டாயிரத்து", 2000 },
            { "மூவாயிரம்", 3000 }, { "மூவாயிரத்து", 3000 },
            { "நாலாயிரம்", 4000 }, { "நான்காயிரம்", 4000 }, { "நான்காயிரத்து", 4000 },
            { "ஐயாயிரம்", 5000 }, { "ஐயாயிரத்து", 5000 },
            { "ஆறாயிரம்", 6000 }, { "ஆறாயிரத்து", 6000 },
            { "ஏழாயிரம்", 7000 }, { "ஏழாயிரத்து", 7000 },
            { "எட்டாயிரம்", 8000 }, { "எட்டாயிரத்து", 8000 },
            { "ஒன்பதாயிரம்", 9000 }, { "ஒன்பதாயிரத்து", 9000 },
            { "பத்தாயிரம்", 10000 }, { "பத்தாயிரத்து", 10000 },
            { "இருபதாயிரம்", 20000 }, { "இருபதாயிரத்து", 20000 },

            // Lakhs & Crores
            { "லட்சம்", 100000 }, { "இலட்சம்", 100000 },
            { "லட்சத்து", 100000 }, { "இலட்சத்து", 100000 },
            { "கோடி", 10000000 }, { "கோடியே", 10000000 }
        };

        public static bool TryParse(string phrase, out long value, out bool isOrdinal)
        {
            value = 0;
            isOrdinal = false;

            if (string.IsNullOrWhiteSpace(phrase))
                return false;

            string clean = phrase.Trim();

            // Detect and strip ordinal suffixes (independent vowel ஆம் or dependent vowel sign ாம்)
            if (clean.EndsWith("-ஆம்"))
            {
                isOrdinal = true;
                clean = clean.Substring(0, clean.Length - 4).Trim();
            }
            else if (clean.EndsWith("ஆம்"))
            {
                isOrdinal = true;
                clean = clean.Substring(0, clean.Length - 3).Trim();
            }
            else if (clean.EndsWith("ாம்"))
            {
                isOrdinal = true;
                clean = clean.Substring(0, clean.Length - 3).Trim();
                // Restore elided final 'ு' after consonant (e.g. இருநூற -> இருநூறு)
                if (DirectWordValues.ContainsKey(clean + "ு"))
                {
                    clean += "ு";
                }
            }
            else if (clean.EndsWith("-வது"))
            {
                isOrdinal = true;
                clean = clean.Substring(0, clean.Length - 4).Trim();
            }
            else if (clean.EndsWith("வது"))
            {
                isOrdinal = true;
                clean = clean.Substring(0, clean.Length - 3).Trim();
            }

            // Direct word match
            if (DirectWordValues.TryGetValue(clean, out long directVal))
            {
                value = directVal;
                return true;
            }

            // If clean ends with 'ற' or 'த்த'
            if (DirectWordValues.TryGetValue(clean + "ு", out long restoredVal))
            {
                value = restoredVal;
                return true;
            }

            // Multi-token parsing (e.g. ஒரு லட்சத்து நாற்பத்து நான்காயிரம்)
            string[] tokens = clean.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                return false;

            long total = 0;
            long currentSegment = 0;

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];

                if (token == "கோடி" || token == "கோடியே")
                {
                    long mult = currentSegment == 0 ? 1 : currentSegment;
                    total += mult * 10000000;
                    currentSegment = 0;
                }
                else if (token == "லட்சம்" || token == "இலட்சம்" || token == "லட்சத்து" || token == "இலட்சத்து")
                {
                    long mult = currentSegment == 0 ? 1 : currentSegment;
                    total += mult * 100000;
                    currentSegment = 0;
                }
                else if (token == "ஆயிரம்" || token == "ஆயிரத்து")
                {
                    long mult = currentSegment == 0 ? 1 : currentSegment;
                    total += mult * 1000;
                    currentSegment = 0;
                }
                else if (token.EndsWith("ஆயிரம்") || token.EndsWith("ாயிரம்") ||
                         token.EndsWith("ஆயிரத்து") || token.EndsWith("ாயிரத்து"))
                {
                    // e.g. நான்காயிரம், ஐயாயிரம், நாற்பத்து நான்காயிரம்
                    long thousandBase = 0;
                    if (DirectWordValues.TryGetValue(token, out long directThousand))
                    {
                        thousandBase = directThousand;
                    }
                    else if (token.EndsWith("நான்காயிரம்"))
                    {
                        thousandBase = 4000;
                    }
                    else if (token.EndsWith("ஆயிரம்") && DirectWordValues.TryGetValue(token.Replace("ஆயிரம்", ""), out long preUnits))
                    {
                        thousandBase = preUnits * 1000;
                    }

                    if (thousandBase > 0)
                    {
                        // Add (currentSegment * 1000) + thousandBase
                        // e.g. நாற்பத்து (40) + நான்காயிரம் (4000) -> (40 * 1000) + 4000 = 44000
                        total += (currentSegment * 1000) + thousandBase;
                        currentSegment = 0;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (DirectWordValues.TryGetValue(token, out long val))
                {
                    currentSegment += val;
                }
                else
                {
                    return false;
                }
            }

            total += currentSegment;
            if (total > 0)
            {
                value = total;
                return true;
            }

            return false;
        }

        public static bool TryConvertPhraseToDigits(string phrase, out string formattedDigits)
        {
            formattedDigits = string.Empty;

            if (TryParse(phrase, out long value, out bool isOrdinal))
            {
                // Rule 5: Only convert to digits if >= 200
                if (value >= 200)
                {
                    string numStr = IndianNumberFormatter.Format(value);
                    if (isOrdinal)
                    {
                        formattedDigits = numStr + "-ஆம்";
                    }
                    else
                    {
                        formattedDigits = numStr;
                    }
                    return true;
                }
            }

            return false;
        }
    }
}
