using System;

namespace NumberConversions.Core
{
    /// <summary>
    /// Converts numbers in the range 0 to 199 from numeric digits into Tamil text words.
    /// </summary>
    public static class TamilCardinalToTextConverter
    {
        public static bool IsInRange(int number) => number >= 0 && number <= 199;

        public static string Convert(int number)
        {
            if (!IsInRange(number))
                throw new ArgumentOutOfRangeException(nameof(number), "Supported range is 0 to 199.");

            if (number < 10)
                return TamilNumberLexicon.Units[number];

            if (number < 20)
                return TamilNumberLexicon.Teens[number - 10];

            if (number < 100)
            {
                int tens = number / 10;
                int units = number % 10;
                if (units == 0)
                    return TamilNumberLexicon.TensBase[tens];

                return GetSandhiTensAndUnits(tens, units);
            }

            // 100 to 199
            if (number == 100)
                return "நூறு";

            int remainder = number - 100;
            string remainderText = Convert(remainder);

            return "நூற்று " + remainderText;
        }

        private static string GetSandhiTensAndUnits(int tens, int units)
        {
            // Standard sandhi combinations for tens + units
            switch (tens)
            {
                case 2: // 20: இருபது
                    switch (units)
                    {
                        case 1: return "இருபத்தொன்று";
                        case 2: return "இருபத்திரண்டு";
                        case 3: return "இருபத்துமூன்று";
                        case 4: return "இருபத்துநான்கு";
                        case 5: return "இருபத்தைந்து";
                        case 6: return "இருபத்தாறு";
                        case 7: return "இருபத்தேழு";
                        case 8: return "இருபத்தெட்டு";
                        case 9: return "இருபத்தொன்பது";
                    }
                    break;

                case 3: // 30: முப்பது
                    switch (units)
                    {
                        case 1: return "முப்பத்தொன்று";
                        case 2: return "முப்பத்திரண்டு";
                        case 3: return "முப்பத்துமூன்று";
                        case 4: return "முப்பத்துநான்கு";
                        case 5: return "முப்பத்தைந்து";
                        case 6: return "முப்பத்தாறு";
                        case 7: return "முப்பத்தேழு";
                        case 8: return "முப்பத்தெட்டு";
                        case 9: return "முப்பத்தொன்பது";
                    }
                    break;

                case 4: // 40: நாற்பது
                    switch (units)
                    {
                        case 1: return "நாற்பத்தொன்று";
                        case 2: return "நாற்பத்திரண்டு";
                        case 3: return "நாற்பத்துமூன்று";
                        case 4: return "நாற்பத்துநான்கு";
                        case 5: return "நாற்பத்தைந்து";
                        case 6: return "நாற்பத்தாறு";
                        case 7: return "நாற்பத்தேழு";
                        case 8: return "நாற்பத்தெட்டு";
                        case 9: return "நாற்பத்தொன்பது";
                    }
                    break;

                case 5: // 50: ஐம்பது
                    switch (units)
                    {
                        case 1: return "ஐம்பத்தொன்று";
                        case 2: return "ஐம்பத்திரண்டு";
                        case 3: return "ஐம்பத்துமூன்று";
                        case 4: return "ஐம்பத்துநான்கு";
                        case 5: return "ஐம்பத்தைந்து";
                        case 6: return "ஐம்பத்தாறு";
                        case 7: return "ஐம்பத்தேழு";
                        case 8: return "ஐம்பத்தெட்டு";
                        case 9: return "ஐம்பத்தொன்பது";
                    }
                    break;

                case 6: // 60: அறுபது
                    switch (units)
                    {
                        case 1: return "அறுபத்தொன்று";
                        case 2: return "அறுபத்திரண்டு";
                        case 3: return "அறுபத்துமூன்று";
                        case 4: return "அறுபத்துநான்கு";
                        case 5: return "அறுபத்தைந்து";
                        case 6: return "அறுபத்தாறு";
                        case 7: return "அறுபத்தேழு";
                        case 8: return "அறுபத்தெட்டு";
                        case 9: return "அறுபத்தொன்பது";
                    }
                    break;

                case 7: // 70: எழுபது
                    switch (units)
                    {
                        case 1: return "எழுபத்தொன்று";
                        case 2: return "எழுபத்திரண்டு";
                        case 3: return "எழுபத்துமூன்று";
                        case 4: return "எழுபத்துநான்கு";
                        case 5: return "எழுபத்தைந்து"; // 175 -> நூற்று எழுபத்தைந்து
                        case 6: return "எழுபத்தாறு";
                        case 7: return "எழுபத்தேழு";
                        case 8: return "எழுபத்தெட்டு";
                        case 9: return "எழுபத்தொன்பது";
                    }
                    break;

                case 8: // 80: எண்பது
                    switch (units)
                    {
                        case 1: return "எண்பத்தொன்று";
                        case 2: return "எண்பத்திரண்டு";
                        case 3: return "எண்பத்துமூன்று";
                        case 4: return "எண்பத்துநான்கு";
                        case 5: return "எண்பத்தைந்து";
                        case 6: return "எண்பத்தாறு";
                        case 7: return "எண்பத்தேழு";
                        case 8: return "எண்பத்தெட்டு";
                        case 9: return "எண்பத்தொன்பது";
                    }
                    break;

                case 9: // 90: தொண்ணூறு
                    switch (units)
                    {
                        case 1: return "தொண்ணூற்றொன்று";
                        case 2: return "தொண்ணூற்றிரண்டு";
                        case 3: return "தொண்ணூற்றுமூன்று";
                        case 4: return "தொண்ணூற்றுநான்கு";
                        case 5: return "தொண்ணூற்றைந்து";
                        case 6: return "தொண்ணூற்றாறு";
                        case 7: return "தொண்ணூற்றேழு";
                        case 8: return "தொண்ணூற்றெட்டு";
                        case 9: return "தொண்ணூற்றொன்பது";
                    }
                    break;
            }

            return TamilNumberLexicon.TensPrefix[tens] + TamilNumberLexicon.Units[units];
        }
    }
}
