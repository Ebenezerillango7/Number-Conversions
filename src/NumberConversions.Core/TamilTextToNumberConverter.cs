using System;
using System.Collections.Generic;

namespace NumberConversions.Core
{
    /// <summary>
    /// Parses Tamil number phrases and words for values >= 200 and converts them to formatted digits,
    /// with built-in support for orthographic variants, alternate spellings, and spelling error detection.
    /// </summary>
    public static class TamilTextToNumberConverter
    {
        private static readonly Dictionary<string, long> DirectWordValues = new Dictionary<string, long>(StringComparer.Ordinal)
        {
            // Digits
            { "பூஜ்யம்", 0 }, { "பூச்சியம்", 0 }, { "பூஜ்ஜியம்", 0 }, { "சுழியம்", 0 },
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
            { "இருபது", 20 }, { "இருபத்து", 20 }, { "இருபத்த", 20 },
            { "முப்பது", 30 }, { "முப்பத்து", 30 }, { "முப்பத்த", 30 },
            { "நாற்பது", 40 }, { "நாற்பத்து", 40 }, { "நாற்பத்த", 40 }, { "நால்பது", 40 },
            { "ஐம்பது", 50 }, { "ஐம்பத்து", 50 }, { "ஐம்பத்த", 50 },
            { "அறுபது", 60 }, { "அறுபத்து", 60 }, { "அறுபத்த", 60 },
            { "எழுபது", 70 }, { "எழுபத்து", 70 }, { "எழுபத்த", 70 },
            { "எண்பது", 80 }, { "எண்பத்து", 80 }, { "எண்பத்த", 80 },
            { "தொண்ணூறு", 90 }, { "தொண்ணூற்று", 90 }, { "தொண்ணூற", 90 },

            // Hundreds (including alternate spelling variants)
            { "நூறு", 100 }, { "நூற்று", 100 }, { "நூற", 100 },
            { "இருநூறு", 200 }, { "இருநூற்று", 200 }, { "இருநூற", 200 },
            { "முந்நூறு", 300 }, { "முந்நூற்று", 300 }, { "முந்நூற", 300 },
            { "முன்னூறு", 300 }, { "முன்னூற்று", 300 }, { "முன்னூற", 300 }, // Alternate spelling
            { "நானூறு", 400 }, { "நானூற்று", 400 }, { "நானூற", 400 },
            { "ஐந்நூறு", 500 }, { "ஐந்நூற்று", 500 }, { "ஐந்நூற", 500 },
            { "ஐநூறு", 500 }, { "ஐநூற்று", 500 }, { "ஐநூற", 500 },         // Alternate spelling
            { "அறுநூறு", 600 }, { "அறுநூற்று", 600 }, { "அறுநூற", 600 },
            { "எழுநூறு", 700 }, { "எழுநூற்று", 700 }, { "எழுநூற", 700 },
            { "எண்ணூறு", 800 }, { "எண்ணூற்று", 800 }, { "எண்ணூற", 800 },
            { "தொள்ளாயிரம்", 900 }, { "தொள்ளாயிரத்து", 900 }, { "தொள்ளாயிரத்த", 900 },

            // Thousands (including alternate orthographic forms: ஐந்தாயிரம் vs ஐயாயிரம், நாலாயிரம் vs நான்காயிரம்)
            { "ஆயிரம்", 1000 }, { "ஆயிரத்து", 1000 }, { "ஆயிரத்த", 1000 }, { "ஓராயிரம்", 1000 },
            { "இரண்டாயிரம்", 2000 }, { "இரண்டாயிரத்து", 2000 }, { "ஈராயிரம்", 2000 },
            { "மூவாயிரம்", 3000 }, { "மூவாயிரத்து", 3000 },
            { "நான்காயிரம்", 4000 }, { "நான்காயிரத்து", 4000 }, { "நாலாயிரம்", 4000 }, { "நாலாயிரத்து", 4000 },
            { "ஐயாயிரம்", 5000 }, { "ஐயாயிரத்து", 5000 },
            { "ஐந்தாயிரம்", 5000 }, { "ஐந்தாயிரத்து", 5000 }, // Alternate form
            { "ஆறாயிரம்", 6000 }, { "ஆறாயிரத்து", 6000 },
            { "ஏழாயிரம்", 7000 }, { "ஏழாயிரத்து", 7000 },
            { "எட்டாயிரம்", 8000 }, { "எட்டாயிரத்து", 8000 },
            { "ஒன்பதாயிரம்", 9000 }, { "ஒன்பதாயிரத்து", 9000 },
            { "பத்தாயிரம்", 10000 }, { "பத்தாயிரத்து", 10000 },
            { "பதினாயிரம்", 10000 }, { "பதினாயிரத்து", 10000 }, { "பதினாயிரத்த", 10000 },
            { "பதினோராயிரம்", 11000 }, { "பதினோராயிரத்து", 11000 },
            { "பன்னீராயிரம்", 12000 }, { "பன்னீராயிரத்து", 12000 },
            { "பதின்மூவாயிரம்", 13000 }, { "பதின்மூவாயிரத்து", 13000 },
            { "பதினான்காயிரம்", 14000 }, { "பதினான்காயிரத்து", 14000 },
            { "பதினைந்தாயிரம்", 15000 }, { "பதினைந்தாயிரத்து", 15000 },
            { "பதினையாயிரம்", 15000 }, { "பதினையாயிரத்து", 15000 },
            { "பதினாறாயிரம்", 16000 }, { "பதினாறாயிரத்து", 16000 },
            { "பதினேழாயிரம்", 17000 }, { "பதினேழாயிரத்து", 17000 },
            { "பதினெட்டாயிரம்", 18000 }, { "பதினெட்டாயிரத்து", 18000 },
            { "பத்தொன்பதாயிரம்", 19000 }, { "பத்தொன்பதாயிரத்து", 19000 },
            { "இருபதாயிரம்", 20000 }, { "இருபதாயிரத்து", 20000 },
            { "முப்பதாயிரம்", 30000 }, { "முப்பத்தாயிரம்", 30000 },
            { "நாற்பதாயிரம்", 40000 }, { "நாற்பத்தாயிரம்", 40000 },
            { "ஐம்பதாயிரம்", 50000 }, { "ஐம்பத்தாயிரம்", 50000 },
            { "அறுபதாயிரம்", 60000 }, { "அறுபத்தாயிரம்", 60000 },
            { "எழுபதாயிரம்", 70000 }, { "எழுபத்தாயிரம்", 70000 },
            { "எண்பதாயிரம்", 80000 }, { "எண்பத்தாயிரம்", 80000 },
            { "தொண்ணூறாயிரம்", 90000 },

            // Lakhs & Crores (including alternate variants)
            { "லட்சம்", 100000 }, { "இலட்சம்", 100000 }, { "லட்சத்து", 100000 }, { "இலட்சத்து", 100000 },
            { "இலக்கம்", 100000 }, { "லச்சம்", 100000 },
            { "கோடி", 10000000 }, { "கோடியே", 10000000 }
        };

        // Common spelling mistakes / missing pulli (e.g. அறுபதைந்து instead of அறுபத்தைந்து)
        private static readonly Dictionary<string, (long Value, string CorrectedSpelling)> CommonTypos = 
            new Dictionary<string, (long, string)>(StringComparer.Ordinal)
        {
            { "இருபதைந்து", (25, "இருபத்தைந்து") },
            { "முப்பதைந்து", (35, "முப்பத்தைந்து") },
            { "நாற்பதைந்து", (45, "நாற்பத்தைந்து") },
            { "ஐம்பதைந்து", (55, "ஐம்பத்தைந்து") },
            { "அறுபதைந்து", (65, "அறுபத்தைந்து") },
            { "எழுபதைந்து", (75, "எழுபத்தைந்து") },
            { "எண்பதைந்து", (85, "எண்பத்தைந்து") },
            { "தொண்ணூறைந்து", (95, "தொண்ணூற்றைந்து") }
        };

        public static bool CheckForKnownTypo(string word, out long value, out string correctedSpelling)
        {
            value = 0;
            correctedSpelling = string.Empty;

            if (string.IsNullOrWhiteSpace(word))
                return false;

            string clean = word.Trim();
            if (CommonTypos.TryGetValue(clean, out var info))
            {
                value = info.Value;
                correctedSpelling = info.CorrectedSpelling;
                return true;
            }

            return false;
        }

        public static bool TryParse(string phrase, out long value, out bool isOrdinal)
        {
            value = 0;
            isOrdinal = false;

            if (string.IsNullOrWhiteSpace(phrase))
                return false;

            string clean = phrase.Trim();

            // Detect and strip ordinal suffixes
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

            // Direct word match with trailing sandhi stripped on oblique forms (e.g. இருநூற்றுப் -> இருநூற்று = 200, ஆயிரத்துப் -> ஆயிரத்து = 1000)
            if (clean.EndsWith("ப்") || clean.EndsWith("த்") || clean.EndsWith("க்") || clean.EndsWith("ச்"))
            {
                string stripped = clean.Substring(0, clean.Length - 2);
                if (DirectWordValues.TryGetValue(stripped, out long strippedVal))
                {
                    value = strippedVal;
                    return true;
                }
            }

            // Restored 'ு'
            if (DirectWordValues.TryGetValue(clean + "ு", out long restoredVal))
            {
                value = restoredVal;
                return true;
            }

            // Multi-token parsing
            string[] tokens = clean.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                return false;

            long total = 0;
            long currentSegment = 0;

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                if (i == tokens.Length - 1 && (token.EndsWith("ப்") || token.EndsWith("த்") || token.EndsWith("க்") || token.EndsWith("ச்")))
                {
                    string candidate = token.Substring(0, token.Length - 2);
                    if (candidate.EndsWith("த்து") || candidate.EndsWith("ற்று") || candidate.EndsWith("ாயிரத்து") || candidate.EndsWith("ாயிரத்த") || DirectWordValues.ContainsKey(candidate))
                    {
                        token = candidate;
                    }
                }

                if (token == "கோடி" || token == "கோடியே")
                {
                    long mult = currentSegment == 0 ? 1 : currentSegment;
                    total += mult * 10000000;
                    currentSegment = 0;
                }
                else if (token == "லட்சம்" || token == "இலட்சம்" || token == "லட்சத்து" || token == "இலட்சத்து" || token == "இலக்கம்" || token == "லச்சம்")
                {
                    long mult = currentSegment == 0 ? 1 : currentSegment;
                    total += mult * 100000;
                    currentSegment = 0;
                }
                else if (token == "ஆயிரம்" || token == "ஆயிரத்து" || token == "ஆயிரத்த")
                {
                    long mult = currentSegment == 0 ? 1 : currentSegment;
                    total += mult * 1000;
                    currentSegment = 0;
                }
                else if (token.EndsWith("ஆயிரம்") || token.EndsWith("ாயிரம்") ||
                         token.EndsWith("ஆயிரத்து") || token.EndsWith("ாயிரத்து"))
                {
                    long thousandBase = 0;
                    if (DirectWordValues.TryGetValue(token, out long directThousand))
                    {
                        thousandBase = directThousand;
                    }
                    else if (token.EndsWith("நான்காயிரம்"))
                    {
                        thousandBase = 4000;
                    }
                    else if (token.EndsWith("ஐந்தாயிரம்") || token.EndsWith("ஐயாயிரம்"))
                    {
                        thousandBase = 5000;
                    }
                    else if (token.EndsWith("ஆயிரம்") && DirectWordValues.TryGetValue(token.Replace("ஆயிரம்", ""), out long preUnits))
                    {
                        thousandBase = preUnits * 1000;
                    }

                    if (thousandBase > 0)
                    {
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
