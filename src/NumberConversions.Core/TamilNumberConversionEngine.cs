using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NumberConversions.Core
{
    /// <summary>
    /// Unified engine for detecting and converting numbers in Tamil scripture body text.
    /// Handles:
    /// 1. 0 - 199: Digit to Tamil text words (cardinal and ordinal).
    /// 2. 200+: Tamil text words to digits with Indian thousand separator (1,23,456,789).
    /// 3. Alternate spellings (e.g. ஐந்தாயிரம் vs ஐயாயிரம்).
    /// 4. Detection of typos / spelling mistakes (e.g. அறுபதைந்து instead of அறுபத்தைந்து).
    /// 5. Detection of malformed separators (e.g. 5.673 or 5, 673 instead of 5,673).
    /// 6. Automatic flagging of all suspicious items (unchecked by default).
    /// </summary>
    public class TamilNumberConversionEngine
    {
        // Matches the entire whole digit number (including Indian/international comma groupings)
        private static readonly Regex WholeDigitNumberPattern = new Regex(
            @"(?<!\d|\d,)(\d{1,3}(?:,\d{2,3})*|\d+)(st|nd|rd|th|-?ஆம்|-?வது|ம்)?(?!\d|,\d)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Regex for malformed separators: e.g. 5.673 (period instead of comma for thousands)
        private static readonly Regex DotAsThousandSeparatorPattern = new Regex(
            @"(?<!\d)(\d{1,3})\.(\d{3})(?!\d)",
            RegexOptions.Compiled);

        // Regex for malformed space after comma: e.g. 5, 673
        private static readonly Regex SpaceAfterCommaSeparatorPattern = new Regex(
            @"(?<!\d)(\d{1,3}),\s+(\d{3})(?!\d)",
            RegexOptions.Compiled);

        // Candidate start words for Tamil numbers
        private static readonly Regex TamilCandidateStartPattern = new Regex(
            @"(?:ஒரு|ஓர்|இரண்டாயிரத்து|இரண்டாயிரம்|இரண்டு|ஈராயிரம்|மூவாயிரத்து|மூவாயிரம்|மூன்று|நான்காயிரத்து|நான்காயிரம்|நான்கு|நாலாயிரத்து|நாலாயிரம்|நாலு|ஐயாயிரத்து|ஐயாயிரம்|ஐந்தாயிரத்து|ஐந்தாயிரம்|ஐந்து|ஆறாயிரத்து|ஆறாயிரம்|ஆறு|ஏழாயிரத்து|ஏழாயிரம்|ஏழு|எட்டாயிரத்து|எட்டாயிரம்|எட்டு|ஒன்பதாயிரத்து|ஒன்பதாயிரம்|ஒன்பது|பத்தாயிரத்து|பத்தாயிரம்|பத்து|இருபது|இருபத்து|இருபத்த|இருபதைந்து|முப்பது|முப்பத்து|முப்பத்த|முப்பதைந்து|முப்பதாயிரம்|முப்பத்தாயிரம்|நாற்பது|நாற்பத்து|நாற்பத்த|நாற்பதைந்து|நாற்பதாயிரம்|நாற்பத்தாயிரம்|நால்பது|ஐம்பது|ஐம்பத்து|ஐம்பத்த|ஐம்பதைந்து|ஐம்பதாயிரம்|ஐம்பத்தாயிரம்|அறுபது|அறுபத்து|அறுபத்த|அறுபதைந்து|அறுபதாயிரம்|அறுபத்தாயிரம்|எழுபது|எழுபத்து|எழுபத்த|எழுபதைந்து|எழுபதாயிரம்|எழுபத்தாயிரம்|எண்பது|எண்பத்து|எண்பத்த|எண்பதைந்து|எண்பதாயிரம்|எண்பத்தாயிரம்|தொண்ணூறு|தொண்ணூற்று|தொண்ணூற|தொண்ணூறைந்து|தொண்ணூறாயிரம்|நூறு|நூற்று|இருநூறு|இருநூற்று|இருநூற|முந்நூறு|முந்நூற்று|முந்நூற|முன்னூறு|முன்னூற்று|முன்னூற|நானூறு|நானூற்று|நானூற|ஐந்நூறு|ஐந்நூற்று|ஐந்நூற|ஐநூறு|ஐநூற்று|ஐநூற|அறுநூறு|அறுநூற்று|அறுநூற|எழுநூறு|எழுநூற்று|எழுநூற|எண்ணூறு|எண்ணூற்று|எண்ணூற|தொள்ளாயிரம்|தொள்ளாயிரத்து|தொள்ளாயிரத்த|ஆயிரம்|ஆயிரத்து|ஆயிரத்த|ஓராயிரம்|லட்சம்|இலட்சம்|லட்சத்து|இலட்சத்து|இலக்கம்|லச்சம்|கோடி|கோடியே)(?:-?ஆம்|-?வது)?",
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

            // 1. Scan for whole digit numbers
            var digitMatches = WholeDigitNumberPattern.Matches(text);
            foreach (Match match in digitMatches)
            {
                int index = match.Index;

                if (ScriptureProtectedEntities.IsProtectedTitlePrefix(text, index))
                    continue;

                string rawNumberStr = match.Groups[1].Value.Replace(",", "");
                if (!long.TryParse(rawNumberStr, out long numVal))
                    continue;

                string suffix = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;

                if (numVal >= 0 && numVal <= 199)
                {
                    string replacement;
                    if (!string.IsNullOrEmpty(suffix))
                    {
                        replacement = TamilOrdinalToTextConverter.Convert((int)numVal);
                    }
                    else
                    {
                        replacement = TamilCardinalToTextConverter.Convert((int)numVal);
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
                        IsAccepted = true,
                        IsSuspicious = false
                    });
                }
                else if (numVal >= 1000)
                {
                    string expectedFormatted = IndianNumberFormatter.Format(numVal);
                    if (!string.IsNullOrEmpty(suffix))
                    {
                        expectedFormatted += (suffix.StartsWith("-") ? suffix : "-" + suffix);
                    }

                    if (match.Value != expectedFormatted)
                    {
                        proposals.Add(new ConversionProposal
                        {
                            Reference = verseRef,
                            BookNum = bookNum,
                            ChapterNum = chapterNum,
                            VerseNum = verseNum,
                            TokenIndex = tokenIndex,
                            TokenOriginalText = text,
                            MatchedToken = match.Value,
                            Replacement = expectedFormatted,
                            StartIndex = match.Index,
                            Length = match.Length,
                            OriginalText = text,
                            RuleType = ConversionRuleType.TextToDigit,
                            IsAccepted = true,
                            IsSuspicious = false
                        });
                    }
                }
            }

            // 2. Scan for Malformed Separator Typos: e.g. 5.673 (dot instead of comma)
            var dotMatches = DotAsThousandSeparatorPattern.Matches(text);
            foreach (Match match in dotMatches)
            {
                string group1 = match.Groups[1].Value;
                string group2 = match.Groups[2].Value;
                string suggested = $"{group1},{group2}";

                proposals.Add(new ConversionProposal
                {
                    Reference = verseRef,
                    BookNum = bookNum,
                    ChapterNum = chapterNum,
                    VerseNum = verseNum,
                    TokenIndex = tokenIndex,
                    TokenOriginalText = text,
                    MatchedToken = match.Value,
                    Replacement = suggested,
                    StartIndex = match.Index,
                    Length = match.Length,
                    OriginalText = text,
                    RuleType = ConversionRuleType.SuspiciousSeparator,
                    IsAccepted = false, // NEVER auto-insert suspicious items
                    IsSuspicious = true,
                    WarningReason = $"Suspicious separator '.' in '{match.Value}' (Likely intended '{suggested}')"
                });
            }

            // 3. Scan for Malformed Spacing in Separator: e.g. 5, 673
            var spaceMatches = SpaceAfterCommaSeparatorPattern.Matches(text);
            foreach (Match match in spaceMatches)
            {
                string group1 = match.Groups[1].Value;
                string group2 = match.Groups[2].Value;
                string suggested = $"{group1},{group2}";

                proposals.Add(new ConversionProposal
                {
                    Reference = verseRef,
                    BookNum = bookNum,
                    ChapterNum = chapterNum,
                    VerseNum = verseNum,
                    TokenIndex = tokenIndex,
                    TokenOriginalText = text,
                    MatchedToken = match.Value,
                    Replacement = suggested,
                    StartIndex = match.Index,
                    Length = match.Length,
                    OriginalText = text,
                    RuleType = ConversionRuleType.SuspiciousSeparator,
                    IsAccepted = false, // NEVER auto-insert suspicious items
                    IsSuspicious = true,
                    WarningReason = $"Suspicious space after comma in '{match.Value}' (Likely intended '{suggested}')"
                });
            }

            // 4. Scan for Tamil number words (200+) and Typos
            int scanIdx = 0;
            while (scanIdx < text.Length)
            {
                var match = TamilCandidateStartPattern.Match(text, scanIdx);
                if (!match.Success)
                    break;

                int startPos = match.Index;

                if (ScriptureProtectedEntities.IsProtectedTitlePrefix(text, startPos))
                {
                    scanIdx = startPos + match.Length;
                    continue;
                }

                // Check for single-word typo (e.g. அறுபதைந்து -> அறுபத்தைந்து)
                string firstWord = match.Value.Trim();
                if (TamilTextToNumberConverter.CheckForKnownTypo(firstWord, out long typoVal, out string correctedSpelling))
                {
                    proposals.Add(new ConversionProposal
                    {
                        Reference = verseRef,
                        BookNum = bookNum,
                        ChapterNum = chapterNum,
                        VerseNum = verseNum,
                        TokenIndex = tokenIndex,
                        TokenOriginalText = text,
                        MatchedToken = firstWord,
                        Replacement = correctedSpelling,
                        StartIndex = startPos,
                        Length = firstWord.Length,
                        OriginalText = text,
                        RuleType = ConversionRuleType.SuspiciousSpelling,
                        IsAccepted = false, // NEVER auto-insert suspicious spelling mistake
                        IsSuspicious = true,
                        WarningReason = $"Spelling mistake: '{firstWord}' (Missing 'த்', intended '{correctedSpelling}' = {typoVal})"
                    });

                    scanIdx = startPos + firstWord.Length;
                    continue;
                }

                // Greedily extend phrase while consecutive words form a valid number phrase
                string bestPhrase = string.Empty;
                string bestDigits = string.Empty;
                int bestLength = 0;

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
                        IsAccepted = true,
                        IsSuspicious = false
                    });

                    scanIdx = startPos + bestLength;
                }
                else
                {
                    scanIdx = startPos + match.Length;
                }
            }

            // Remove any overlapping proposals by keeping the most specific match
            proposals = proposals
                .GroupBy(p => p.StartIndex)
                .Select(g => g.OrderByDescending(p => p.Length).First())
                .OrderBy(p => p.StartIndex)
                .ToList();

            // Populate ProposedText preview for each proposal
            string fullProposed = ApplyProposals(text, proposals.Where(p => p.IsAccepted && !p.IsSuspicious));
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
            // Only apply approved proposals that are confirmed by user
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
