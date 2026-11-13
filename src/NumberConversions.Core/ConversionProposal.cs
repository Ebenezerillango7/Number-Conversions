using System;

namespace NumberConversions.Core
{
    public enum ConversionRuleType
    {
        DigitToText,          // 0 - 199 to Tamil Words
        TextToDigit,          // 200+ to Formatted Digits
        SandhiAdjusted,       // Sandhi added or removed at word boundary (புணர்ச்சி)
        AlternateSpelling,    // Alternate / Dialectal spelling normalized (e.g. ஐந்தாயிரம் vs ஐயாயிரம்)
        SuspiciousSpelling,   // Suspected typo / spelling mistake (e.g. அறுபதைந்து instead of அறுபத்தைந்து)
        SuspiciousSeparator   // Suspected wrong separator (e.g. 5.673 or 5, 673 instead of 5,673)
    }

    /// <summary>
    /// Represents a discrete number replacement candidate in a specific scripture verse.
    /// </summary>
    public class ConversionProposal
    {
        public string Reference { get; set; } = string.Empty;
        public int BookNum { get; set; }
        public int ChapterNum { get; set; }
        public int VerseNum { get; set; }
        public int TokenIndex { get; set; } // Exact token position in the chapter token stream
        public string TokenOriginalText { get; set; } = string.Empty;

        public string MatchedToken { get; set; } = string.Empty;
        public string Replacement { get; set; } = string.Empty;
        public int StartIndex { get; set; }
        public int Length { get; set; }

        public string OriginalText { get; set; } = string.Empty;
        public string ProposedText { get; set; } = string.Empty;

        public ConversionRuleType RuleType { get; set; }
        public bool IsAccepted { get; set; } = true;

        // Suspicious / Ambiguity Flags (Translators must review; NEVER auto-inserted)
        public bool IsSuspicious { get; set; } = false;
        public string WarningReason { get; set; } = string.Empty;

        public override string ToString()
        {
            string flag = IsSuspicious ? $"[FLAGGED: {WarningReason}] " : "";
            return $"{flag}[{Reference} Token#{TokenIndex}] '{MatchedToken}' -> '{Replacement}' ({RuleType})";
        }
    }
}
