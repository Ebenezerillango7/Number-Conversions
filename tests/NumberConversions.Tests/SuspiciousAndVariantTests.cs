using System.Linq;
using NumberConversions.Core;
using Xunit;

namespace NumberConversions.Tests
{
    public class SuspiciousAndVariantTests
    {
        [Fact]
        public void SpellingMistake_Arupathainthu_IsDetectedAndFlaggedUnchecked()
        {
            var engine = new TamilNumberConversionEngine();

            // Typo: அறுபதைந்து instead of அறுபத்தைந்து (missing 'த்')
            string text = "அவன் அறுபதைந்து வயதானபோது மகனைப் பெற்றான்.";
            var proposals = engine.ScanText(text);

            Assert.Single(proposals);
            var p = proposals[0];

            Assert.Equal("அறுபதைந்து", p.MatchedToken);
            Assert.Equal("அறுபத்தைந்து", p.Replacement);
            Assert.Equal(ConversionRuleType.SuspiciousSpelling, p.RuleType);
            Assert.True(p.IsSuspicious);
            Assert.False(p.IsAccepted); // Must be UNCHECKED by default
            Assert.Contains("Spelling mistake", p.WarningReason);

            // Verifying safety: Since IsAccepted is false, ApplyProposals must NOT touch the text!
            string result = engine.ApplyProposals(text, proposals);
            Assert.Equal(text, result);
        }

        [Theory]
        [InlineData("ஐயாயிரம்", "5,000")]
        [InlineData("ஐந்தாயிரம்", "5,000")]
        [InlineData("நான்காயிரம்", "4,000")]
        [InlineData("நாலாயிரம்", "4,000")]
        [InlineData("இரண்டாயிரம்", "2,000")]
        [InlineData("ஈராயிரம்", "2,000")]
        [InlineData("முந்நூறு", "300")]
        [InlineData("முன்னூறு", "300")]
        [InlineData("ஐந்நூறு", "500")]
        [InlineData("ஐநூறு", "500")]
        public void AlternateSpellings_AreBothRecognizedCleanly(string inputWord, string expectedDigits)
        {
            var engine = new TamilNumberConversionEngine();
            string text = $"அங்கே {inputWord} வீரர்கள் இருந்தார்கள்.";

            var proposals = engine.ScanText(text);
            Assert.Single(proposals);
            Assert.Equal(inputWord, proposals[0].MatchedToken);
            Assert.Equal(expectedDigits, proposals[0].Replacement);
            Assert.True(proposals[0].IsAccepted);
            Assert.False(proposals[0].IsSuspicious);
        }

        [Fact]
        public void MalformedSeparator_DotInsteadOfComma_IsFlaggedUnchecked()
        {
            var engine = new TamilNumberConversionEngine();

            // Typo: 5.673 instead of 5,673
            string text = "மொத்தம் 5.673 போர்வீரர்கள் கூடினார்கள்.";
            var proposals = engine.ScanText(text);

            Assert.Single(proposals);
            var p = proposals[0];

            Assert.Equal("5.673", p.MatchedToken);
            Assert.Equal("5,673", p.Replacement);
            Assert.Equal(ConversionRuleType.SuspiciousSeparator, p.RuleType);
            Assert.True(p.IsSuspicious);
            Assert.False(p.IsAccepted); // Must be UNCHECKED by default
            Assert.Contains("Suspicious separator '.'", p.WarningReason);

            // Verifying safety: It is NOT inserted automatically
            string result = engine.ApplyProposals(text, proposals);
            Assert.Equal(text, result);
        }

        [Fact]
        public void MalformedSeparator_SpaceAfterComma_IsFlaggedUnchecked()
        {
            var engine = new TamilNumberConversionEngine();

            // Typo: 5, 673 instead of 5,673
            string text = "அங்கே 5, 673 நபர்கள் இருந்தனர்.";
            var proposals = engine.ScanText(text);

            Assert.Single(proposals);
            var p = proposals[0];

            Assert.Equal("5, 673", p.MatchedToken);
            Assert.Equal("5,673", p.Replacement);
            Assert.Equal(ConversionRuleType.SuspiciousSeparator, p.RuleType);
            Assert.True(p.IsSuspicious);
            Assert.False(p.IsAccepted); // Must be UNCHECKED by default
            Assert.Contains("Suspicious space after comma", p.WarningReason);

            // Verifying safety: It is NOT inserted automatically
            string result = engine.ApplyProposals(text, proposals);
            Assert.Equal(text, result);
        }

        [Fact]
        public void SuspiciousItems_RequireExplicitUserAcceptanceToApply()
        {
            var engine = new TamilNumberConversionEngine();
            string text = "மொத்தம் 5.673 போர்வீரர்கள்.";

            var proposals = engine.ScanText(text);
            var p = proposals[0];
            Assert.False(p.IsAccepted);

            // 1. Without user acceptance: unchanged
            string unacceptedResult = engine.ApplyProposals(text, proposals);
            Assert.Equal(text, unacceptedResult);

            // 2. When translator explicitly reviews and checks the item:
            p.IsAccepted = true;
            string acceptedResult = engine.ApplyProposals(text, proposals);
            Assert.Equal("மொத்தம் 5,673 போர்வீரர்கள்.", acceptedResult);
        }
    }
}
