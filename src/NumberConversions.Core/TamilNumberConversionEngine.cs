using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NumberConversions.Core
{
    /// <summary>
    /// Unified engine for detecting and converting numbers in Tamil scripture body text.
    /// Implements:
    /// 1. 0 - 199: Digit to Tamil text words (cardinal and ordinal).
    /// 2. 200 and above: Tamil text words to digits with Indian thousand separator (1,23,456,789).
    /// </summary>
    public class TamilNumberConversionEngine
    {
        private static readonly Regex DigitPattern = new Regex(
            @"(?<!\d)(\d{1,3})(st|nd|rd|th|-?ஆம்|-?வது|ம்)?(?!\d)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Regex to identify starting candidate tokens for Tamil numbers
        private static readonly Regex TamilCandidateStartPattern = new Regex(
            @"(?:ஒரு|ஓர்|இரண்டு|மூன்று|நான்கு|ஐந்து|ஆறு|ஏழு|எட்டு|ஒன்பது|பத்து|இருபது|இருபத்து|முப்பது|முப்பத்து|நாற்பது|நாற்பத்து|ஐம்பது|ஐம்பத்து|அறுபது|அறுபத்து|எழுபது|எழுபத்து|எண்பது|எண்பத்து|தொண்ணூறு|தொண்ணூற்று|நூறு|நூற்று|இருநூறு|இருநூற்று|இருநூற|முந்நூறு|முந்நூற்று|முந்நூற|நானூறு|நானூற்று|நானூற|ஐந்நூறு|ஐந்நூற்று|ஐந்நூற|அறுநூறு|அறுநூற்று|அறுநூற|எழுநூறு|எழுநூற்று|எழுநூற|எண்ணூறு|எண்ணூற்று|எண்ணூற|தொள்ளாயிரம்|தொள்ளாயிரத்து|தொள்ளாயிரத்த|ஆயிரம்|ஆயிரத்து|ஆயிரத்த|ஓராயிரம்|இரண்டாயிரம்|இரண்டாயிரத்து|மூவாயிரம்|மூவாயிரத்து|நாலாயிரம்|நான்காயிரம்|நான்காயிரத்து|ஐயாயிரம்|ஐயாயிரத்து|ஆறாயிரம்|ஆறாயிரத்து|ஏழாயிரம்|ஏழாயிரத்து|எட்டாயிரம்|எட்டாயிரத்து|ஒன்பதாயிரம்|ஒன்பதாயிரத்து|பத்தாயிரம்|பத்தாயிரத்து|இருபதாயிரம்|இருபதாயிரத்து|லட்சம்|இலட்சம்|லட்சத்து|இலட்சத்து|கோடி|கோடியே)(?:-?ஆம்|-?வது)?",
            RegexOptions.Compiled);

        public List<ConversionProposal> ScanText(
            string text,
            string verseRef = "",
            int bookNum = 0,
            int chapterNum = 0,
            int verseNum = 0,
            int tokenIndex = 0)
        {
            var proposals = new List<ConversionProposal>();
            if (string.IsNullOrWhiteSpace(text))
                return proposals;

            // 1. Scan for digits (0 - 199) -> Text
            var digitMatches = DigitPattern.Matches(text);
            foreach (Match match in digitMatches)
            {
                int index = match.Index;

                // Check if this digit is part of a protected book name (e.g. 1 சாமுவேல்)
                if (ScriptureProtectedEntities.IsProtectedTitlePrefix(text, index))
                    continue;

                string rawNumberStr = match.Groups[1].Value;
                if (!int.TryParse(rawNumberStr, out int numVal))
                    continue;

                // Rule 5: 0 - 199 digit to text
                if (numVal < 0 || numVal > 199)
                    continue;

                string replacement;
                string suffix = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;

                if (!string.IsNullOrEmpty(suffix))
                {
                    // Ordinal conversion
                    replacement = TamilOrdinalToTextConverter.Convert(numVal);
                }
                else
                {
                    // Cardinal conversion
                    replacement = TamilCardinalToTextConverter.Convert(numVal);
                }

                proposals.Add(new ConversionProposal
                {
                    Reference = verseRef,
                    BookNum = bookNum,
                    ChapterNum = chapterNum,
                    VerseNum = verseNum,
                    TokenIndex = tokenIndex,
                    TokenOriginalText = text,
                    MatchedToken = match.Value,
                    Replacement = replacement,
                    StartIndex = match.Index,
                    Length = match.Length,
                    OriginalText = text,
                    RuleType = ConversionRuleType.DigitToText,
                    IsAccepted = true
                });
            }

            // 2. Scan for Tamil number words (200+) -> Digits
            int scanIdx = 0;
            while (scanIdx < text.Length)
            {
                var match = TamilCandidateStartPattern.Match(text, scanIdx);
                if (!match.Success)
                    break;

                int startPos = match.Index;

                // Check if preceded by protected prefix (e.g. book title)
                if (ScriptureProtectedEntities.IsProtectedTitlePrefix(text, startPos))
                {
                    scanIdx = startPos + match.Length;
                    continue;
                }

                // Greedily extend phrase while consecutive words form a valid number phrase
                string bestPhrase = string.Empty;
                string bestDigits = string.Empty;
                int bestLength = 0;

                // Extract up to 6 consecutive whitespace-delimited words
                string remainder = text.Substring(startPos);
                string[] words = remainder.Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

                string runningPhrase = "";
                for (int w = 0; w < Math.Min(words.Length, 6); w++)
                {
                    if (w > 0) runningPhrase += " ";
                    runningPhrase += words[w];

                    if (TamilTextToNumberConverter.TryConvertPhraseToDigits(runningPhrase, out string digits))
                    {
                        bestPhrase = runningPhrase;
                        bestDigits = digits;
                        bestLength = runningPhrase.Length;
                    }
                }

                if (!string.IsNullOrEmpty(bestPhrase))
                {
                    proposals.Add(new ConversionProposal
                    {
                        Reference = verseRef,
                        BookNum = bookNum,
                        ChapterNum = chapterNum,
                        VerseNum = verseNum,
                        TokenIndex = tokenIndex,
                        TokenOriginalText = text,
                        MatchedToken = bestPhrase,
                        Replacement = bestDigits,
                        StartIndex = startPos,
                        Length = bestLength,
                        OriginalText = text,
                        RuleType = ConversionRuleType.TextToDigit,
                        IsAccepted = true
                    });

                    scanIdx = startPos + bestLength;
                }
                else
                {
                    scanIdx = startPos + match.Length;
                }
            }

            // Sort by StartIndex ascending
            proposals = proposals.OrderBy(p => p.StartIndex).ToList();

            // Populate ProposedText preview for each proposal
            string fullProposed = ApplyProposals(text, proposals.Where(p => p.IsAccepted));
            foreach (var p in proposals)
            {
                p.ProposedText = fullProposed;
            }

            return proposals;
        }

        public string ApplyProposals(string originalText, IEnumerable<ConversionProposal> approvedProposals)
        {
            if (string.IsNullOrEmpty(originalText))
                return originalText;

            // Apply in reverse order so string indices remain valid
            var ordered = approvedProposals
                .Where(p => p.IsAccepted)
                .OrderByDescending(p => p.StartIndex)
                .ToList();

            string result = originalText;
            foreach (var proposal in ordered)
            {
                // Boundary check
                if (proposal.StartIndex < 0 || proposal.StartIndex + proposal.Length > result.Length)
                    continue;

                // CRITICAL SAFETY INVARIANT: The characters at StartIndex MUST match MatchedToken exactly!
                string actualTextAtIndex = result.Substring(proposal.StartIndex, proposal.Length);
                if (actualTextAtIndex != proposal.MatchedToken)
                {
                    // Never replace if target slice does not match MatchedToken
                    continue;
                }

                result = result.Substring(0, proposal.StartIndex) +
                         proposal.Replacement +
                         result.Substring(proposal.StartIndex + proposal.Length);
            }

            return result;
        }
    }
}
