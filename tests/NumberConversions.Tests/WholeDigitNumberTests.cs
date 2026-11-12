using System.Linq;
using NumberConversions.Core;
using Xunit;

namespace NumberConversions.Tests
{
    public class WholeDigitNumberTests
    {
        [Fact]
        public void FormattedThousandNumber_DoesNotSlicePartialDigits()
        {
            var engine = new TamilNumberConversionEngine();

            // "1,000" must be treated as a single whole number, NEVER as "1" and "000"
            string text = "அங்கே 1,000 ஆடுகள் இருந்தன.";
            var proposals = engine.ScanText(text);

            // 1,000 is already formatted and >= 200, so zero proposals
            Assert.Empty(proposals);
        }

        [Fact]
        public void UnformattedThousandNumber_ProposesIndianFormatting()
        {
            var engine = new TamilNumberConversionEngine();

            // Unformatted 1000 -> proposes 1,000
            string text = "அங்கே 1000 ஆடுகள் இருந்தன.";
            var proposals = engine.ScanText(text);

            Assert.Single(proposals);
            Assert.Equal("1000", proposals[0].MatchedToken);
            Assert.Equal("1,000", proposals[0].Replacement);
            Assert.Equal(ConversionRuleType.TextToDigit, proposals[0].RuleType);
        }

        [Fact]
        public void FormattedLakhNumber_IsTreatedAsAtomicWholeNumber()
        {
            var engine = new TamilNumberConversionEngine();

            // "1,44,000" must never match "1", "44", or "000"
            string text = "முத்திரையிடப்பட்டவர்கள் 1,44,000 பேர்.";
            var proposals = engine.ScanText(text);

            Assert.Empty(proposals);
        }

        [Fact]
        public void CommaSeparatedList_ConvertsEachNumberIndependently()
        {
            var engine = new TamilNumberConversionEngine();

            // Grammatical commas between numbers in a list
            string text = "அவன் 12, 14, 16 ஆடுகளைக் கொண்டுவந்தான்.";
            var proposals = engine.ScanText(text);

            Assert.Equal(3, proposals.Count);
            Assert.Equal("12", proposals[0].MatchedToken);
            Assert.Equal("பன்னிரண்டு", proposals[0].Replacement);

            Assert.Equal("14", proposals[1].MatchedToken);
            Assert.Equal("பதினான்கு", proposals[1].Replacement);

            Assert.Equal("16", proposals[2].MatchedToken);
            Assert.Equal("பதினாறு", proposals[2].Replacement);

            string result = engine.ApplyProposals(text, proposals);
            Assert.Equal("அவன் பன்னிரண்டு, பதினான்கு, பதினாறு ஆடுகளைக் கொண்டுவந்தான்.", result);
        }

        [Fact]
        public void PunctuationBoundaries_NumbersNextToPunctuationAreHandledCleanly()
        {
            var engine = new TamilNumberConversionEngine();

            // Comma immediately after number: "வயது 65, அவன்"
            string text = "அவன் வயது 65, அவன் இறந்தான்.";
            var proposals = engine.ScanText(text);

            Assert.Single(proposals);
            Assert.Equal("65", proposals[0].MatchedToken);
            Assert.Equal("அறுபத்தைந்து", proposals[0].Replacement);

            string result = engine.ApplyProposals(text, proposals);
            Assert.Equal("அவன் வயது அறுபத்தைந்து, அவன் இறந்தான்.", result);
        }

        [Fact]
        public void RangeHyphen_BothNumbersAreConvertedCleanly()
        {
            var engine = new TamilNumberConversionEngine();

            string text = "அத்தியாயங்கள் 12-14 வாசிக்கவும்.";
            var proposals = engine.ScanText(text);

            Assert.Equal(2, proposals.Count);
            Assert.Equal("12", proposals[0].MatchedToken);
            Assert.Equal("பன்னிரண்டு", proposals[0].Replacement);

            Assert.Equal("14", proposals[1].MatchedToken);
            Assert.Equal("பதினான்கு", proposals[1].Replacement);

            string result = engine.ApplyProposals(text, proposals);
            Assert.Equal("அத்தியாயங்கள் பன்னிரண்டு-பதினான்கு வாசிக்கவும்.", result);
        }
    }
}
