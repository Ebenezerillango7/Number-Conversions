using System;
using System.Collections.Generic;

namespace NumberConversions.Core
{
    public static class TamilNumberLexicon
    {
        // 0 to 9 cardinal words
        public static readonly string[] Units = new string[]
        {
            "பூஜ்யம்", // 0
            "ஒன்று",   // 1
            "இரண்டு",  // 2
            "மூன்று",  // 3
            "நான்கு",  // 4
            "ஐந்து",   // 5
            "ஆறு",     // 6
            "ஏழு",     // 7
            "எட்டு",   // 8
            "ஒன்பது"   // 9
        };

        // 10 to 19 cardinal words
        public static readonly string[] Teens = new string[]
        {
            "பத்து",        // 10
            "பதினொன்று",    // 11
            "பன்னிரண்டு",   // 12
            "பதின்மூன்று",   // 13
            "பதினான்கு",    // 14
            "பதினைந்து",    // 15
            "பதினாறு",      // 16
            "பதினேழு",      // 17
            "பதினெட்டு",    // 18
            "பத்தொன்பது"    // 19
        };

        // Tens base names (20, 30, 40, 50, 60, 70, 80, 90)
        public static readonly string[] TensBase = new string[]
        {
            "", "",
            "இருபது",   // 20
            "முப்பது",   // 30
            "நாற்பது",  // 40
            "ஐம்பது",   // 50
            "அறுபது",   // 60
            "எழுபது",   // 70
            "எண்பது",   // 80
            "தொண்ணூறு"  // 90
        };

        // Tens combining prefixes (e.g. இருபத்து + ஒன்று -> இருபத்தொன்று / இருபத்து ஒன்று)
        public static readonly string[] TensPrefix = new string[]
        {
            "", "",
            "இருபத்து ",
            "முப்பத்து ",
            "நாற்பத்து ",
            "ஐம்பத்து ",
            "அறுபத்து ",
            "எழுபத்து ",
            "எண்பத்து ",
            "தொண்ணூற்று "
        };

        // Units when combined with tens (sandhi forms e.g. இருபத்தொன்று, இருபத்து இரண்டு, இருபத்து ஐந்து / இருபத்தைந்து)
        public static readonly string[] CombiningUnits = new string[]
        {
            "",
            "ஒன்று",
            "இரண்டு",
            "மூன்று",
            "நான்கு",
            "ஐந்து",
            "ஆறு",
            "ஏழு",
            "எட்டு",
            "ஒன்பது"
        };

        // Ordinal forms for 1 to 10
        public static readonly string[] UnitOrdinals = new string[]
        {
            "பூஜ்யமாவது",
            "முதலாம்",
            "இரண்டாம்",
            "மூன்றாம்",
            "நான்காம்",
            "ஐந்தாம்",
            "ஆறாம்",
            "ஏழாம்",
            "எட்டாம்",
            "ஒன்பதாம்",
            "பத்தாம்"
        };

        // Hundreds names (100 to 900)
        public static readonly Dictionary<int, string> Hundreds = new Dictionary<int, string>()
        {
            { 100, "நூறு" },
            { 200, "இருநூறு" },
            { 300, "முந்நூறு" },
            { 400, "நானூறு" },
            { 500, "ஐந்நூறு" },
            { 600, "அறுநூறு" },
            { 700, "எழுநூறு" },
            { 800, "எண்ணூறு" },
            { 900, "தொள்ளாயிரம்" }
        };

        // Hundreds combining prefixes
        public static readonly Dictionary<int, string> HundredsPrefix = new Dictionary<int, string>()
        {
            { 100, "நூற்று " },
            { 200, "இருநூற்று " },
            { 300, "முந்நூற்று " },
            { 400, "நானூற்று " },
            { 500, "ஐந்நூற்று " },
            { 600, "அறுநூற்று " },
            { 700, "எழுநூற்று " },
            { 800, "எண்ணூற்று " },
            { 900, "தொள்ளாயிரத்து " }
        };

        // Multipliers
        public static readonly Dictionary<string, long> Multipliers = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
            { "ஆயிரத்து", 1000 },
            { "ஆயிரம்", 1000 },
            { "ஓராயிரம்", 1000 },
            { "பத்தாயிரம்", 10000 },
            { "லட்சத்து", 100000 },
            { "இலட்சத்து", 100000 },
            { "லட்சம்", 100000 },
            { "இலட்சம்", 100000 },
            { "கோடியே", 10000000 },
            { "கோடி", 10000000 }
        };
    }
}
