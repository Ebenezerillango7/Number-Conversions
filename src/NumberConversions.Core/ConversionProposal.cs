using System;

namespace NumberConversions.Core
{
    public enum ConversionRuleType
    {
        DigitToText, // 0 - 199 to Tamil Words
        TextToDigit  // 200+ to Formatted Digits
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

        public string MatchedToken { get; set; } = string.Empty;
        public string Replacement { get; set; } = string.Empty;
        public int StartIndex { get; set; }
        public int Length { get; set; }

        public string OriginalText { get; set; } = string.Empty;
        public string ProposedText { get; set; } = string.Empty;

        public ConversionRuleType RuleType { get; set; }
        public bool IsAccepted { get; set; } = true;

        public override string ToString()
        {
            return $"[{Reference}] '{MatchedToken}' -> '{Replacement}' ({RuleType})";
        }
    }
}
