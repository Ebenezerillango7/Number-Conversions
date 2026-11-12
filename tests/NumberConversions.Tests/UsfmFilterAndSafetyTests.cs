using System.Linq;
using NumberConversions.Core;
using Xunit;

namespace NumberConversions.Tests
{
    public class UsfmFilterAndSafetyTests
    {
        [Theory]
        [InlineData("c")]
        [InlineData("v")]
        [InlineData("r")]
        [InlineData("x")]
        [InlineData("f")]
        [InlineData("fe")]
        [InlineData("fig")]
        [InlineData("id")]
        [InlineData("h")]
        [InlineData("toc1")]
        [InlineData("mt1")]
        [InlineData("\\c")]
        [InlineData("\\v")]
        [InlineData("\\r")]
        [InlineData("\\f")]
        [InlineData("\\fig")]
        public void ExcludedMarkers_AreStrictlyIdentified(string marker)
        {
            Assert.True(UsfmTokenFilter.ShouldExcludeMarker(marker));
        }

        [Theory]
        [InlineData("p")]
        [InlineData("m")]
        [InlineData("q1")]
        [InlineData("q2")]
        [InlineData("s")]
        [InlineData("\\p")]
        [InlineData("\\q1")]
        public void PublishableScriptureParagraphs_AreRecognized(string marker)
        {
            Assert.True(UsfmTokenFilter.IsPublishableScriptureParagraph(marker));
        }

        [Fact]
        public void ProtectedBookNames_AreNeverConverted()
        {
            var engine = new TamilNumberConversionEngine();
            string text = "1 சாமுவேல் புத்தகம் மற்றும் 2 கொரிந்தியர் நிருபம்";

            var proposals = engine.ScanText(text);

            // Neither the '1' in '1 சாமுவேல்' nor '2' in '2 கொரிந்தியர்' should be proposed for replacement
            Assert.Empty(proposals);
        }

        [Fact]
        public void FullConversionScenario_EndToEnd()
        {
            var engine = new TamilNumberConversionEngine();

            // Narrative text containing 175, 2nd, and இருநூறு
            string input = "அவன் 175 ஆண்டுகள் வாழ்ந்தான். 2-ஆம் நாளில் இருநூறு பேரைச் சேர்த்தான்.";
            
            var proposals = engine.ScanText(input);

            Assert.Equal(3, proposals.Count);

            // 175 -> நூற்று எழுபத்தைந்து (Digit to Text)
            var p175 = proposals.First(p => p.MatchedToken == "175");
            Assert.Equal(ConversionRuleType.DigitToText, p175.RuleType);
            Assert.Equal("நூற்று எழுபத்தைந்து", p175.Replacement);

            // 2-ஆம் -> இரண்டாம் (Ordinal Digit to Text)
            var p2nd = proposals.First(p => p.MatchedToken == "2-ஆம்");
            Assert.Equal(ConversionRuleType.DigitToText, p2nd.RuleType);
            Assert.Equal("இரண்டாம்", p2nd.Replacement);

            // இருநூறு -> 200 (Text to Digit >= 200)
            var p200 = proposals.First(p => p.MatchedToken == "இருநூறு");
            Assert.Equal(ConversionRuleType.TextToDigit, p200.RuleType);
            Assert.Equal("200", p200.Replacement);

            // Apply conversions
            string converted = engine.ApplyProposals(input, proposals);
            Assert.Equal("அவன் நூற்று எழுபத்தைந்து ஆண்டுகள் வாழ்ந்தான். இரண்டாம் நாளில் 200 பேரைச் சேர்த்தான்.", converted);

            // Idempotency check: Running again on converted text produces 0 proposals!
            var secondPass = engine.ScanText(converted);
            Assert.Empty(secondPass);
        }
    }
}
