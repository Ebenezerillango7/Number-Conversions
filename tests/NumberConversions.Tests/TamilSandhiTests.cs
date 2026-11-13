using System.Collections.Generic;
using NumberConversions.Core;
using Xunit;

namespace NumberConversions.Tests
{
    public class TamilSandhiTests
    {
        private readonly TamilNumberConversionEngine _engine = new TamilNumberConversionEngine();

        [Fact]
        public void Judges_3_29_Removes_Obsolete_Preceding_Sandhi()
        {
            // The exact client verse from Judges 3:29
            string input = "அக்காலத்திலே மோவாபியர்களில் ஏறக்குறையப் பதினாயிரம் பேரை வெட்டினார்கள்";
            var proposals = _engine.ScanText(input, "JDG 3:29", 7, 3, 29, 0, adjustSandhi: true);

            Assert.Single(proposals);
            var p = proposals[0];
            Assert.Equal("ஏறக்குறையப் பதினாயிரம்", p.MatchedToken);
            Assert.Equal("ஏறக்குறைய 10,000", p.Replacement);
            Assert.Equal(ConversionRuleType.SandhiAdjusted, p.RuleType);
            Assert.Contains("Removed obsolete trailing 'ப்'", p.WarningReason);

            string result = _engine.ApplyProposals(input, proposals);
            Assert.Equal("அக்காலத்திலே மோவாபியர்களில் ஏறக்குறைய 10,000 பேரை வெட்டினார்கள்", result);
        }

        [Fact]
        public void Kittathatta_Removes_Obsolete_Sandhi_Before_Digits()
        {
            string input = "அங்கே கிட்டத்தட்டப் ஐயாயிரம் பேர் கூடினார்கள்";
            var proposals = _engine.ScanText(input, "TST 1:1", 1, 1, 1, 0, adjustSandhi: true);

            Assert.Single(proposals);
            Assert.Equal("கிட்டத்தட்டப் ஐயாயிரம்", proposals[0].MatchedToken);
            Assert.Equal("கிட்டத்தட்ட 5,000", proposals[0].Replacement);

            string result = _engine.ApplyProposals(input, proposals);
            Assert.Equal("அங்கே கிட்டத்தட்ட 5,000 பேர் கூடினார்கள்", result);
        }

        [Fact]
        public void Andha_Removes_Obsolete_Sandhi_Before_Digits()
        {
            string input = "அந்தப் பதினாயிரம் பேரை அழைத்தார்";
            var proposals = _engine.ScanText(input, "TST 1:2", 1, 1, 2, 0, adjustSandhi: true);

            Assert.Single(proposals);
            Assert.Equal("அந்தப் பதினாயிரம்", proposals[0].MatchedToken);
            Assert.Equal("அந்த 10,000", proposals[0].Replacement);

            string result = _engine.ApplyProposals(input, proposals);
            Assert.Equal("அந்த 10,000 பேரை அழைத்தார்", result);
        }

        [Fact]
        public void Oblique_Hundreds_Removes_Trailing_Sandhi()
        {
            string input = "அங்கே இருநூற்றுப் பேர் இருந்தார்கள்";
            var proposals = _engine.ScanText(input, "TST 1:3", 1, 1, 3, 0, adjustSandhi: true);

            Assert.Single(proposals);
            Assert.Equal("இருநூற்றுப்", proposals[0].MatchedToken);
            Assert.Equal("200", proposals[0].Replacement);

            string result = _engine.ApplyProposals(input, proposals);
            Assert.Equal("அங்கே 200 பேர் இருந்தார்கள்", result);
        }

        [Fact]
        public void Oblique_Thousands_Removes_Trailing_Sandhi()
        {
            string input = "ஆயிரத்துப் பேர் யுத்தத்திற்கு புறப்பட்டார்கள்";
            var proposals = _engine.ScanText(input, "TST 1:4", 1, 1, 4, 0, adjustSandhi: true);

            Assert.Single(proposals);
            Assert.Equal("ஆயிரத்துப்", proposals[0].MatchedToken);
            Assert.Equal("1,000", proposals[0].Replacement);

            string result = _engine.ApplyProposals(input, proposals);
            Assert.Equal("1,000 பேர் யுத்தத்திற்கு புறப்பட்டார்கள்", result);
        }

        [Fact]
        public void Digits_To_Words_Adds_Preceding_And_Following_Sandhi()
        {
            string input = "ஏறக்குறைய 10 பேரை கண்டார்கள்";
            var proposals = _engine.ScanText(input, "TST 1:5", 1, 1, 5, 0, adjustSandhi: true);

            Assert.Single(proposals);
            Assert.Equal("ஏறக்குறைய 10", proposals[0].MatchedToken);
            Assert.Equal("ஏறக்குறையப் பத்துப்", proposals[0].Replacement);
            Assert.Equal(ConversionRuleType.SandhiAdjusted, proposals[0].RuleType);

            string result = _engine.ApplyProposals(input, proposals);
            Assert.Equal("ஏறக்குறையப் பத்துப் பேரை கண்டார்கள்", result);
        }

        [Fact]
        public void Digits_To_Words_Adds_Following_Sandhi_Before_Kumaran()
        {
            string input = "அவனுக்கு 10 குமாரர் இருந்தார்கள்";
            var proposals = _engine.ScanText(input, "TST 1:6", 1, 1, 6, 0, adjustSandhi: true);

            Assert.Single(proposals);
            Assert.Equal("10", proposals[0].MatchedToken);
            Assert.Equal("பத்துக்", proposals[0].Replacement);

            string result = _engine.ApplyProposals(input, proposals);
            Assert.Equal("அவனுக்கு பத்துக் குமாரர் இருந்தார்கள்", result);
        }

        [Fact]
        public void AdjustSandhi_Disabled_Preserves_Raw_Replacement()
        {
            string input = "அக்காலத்திலே மோவாபியர்களில் ஏறக்குறையப் பதினாயிரம் பேரை வெட்டினார்கள்";
            var proposals = _engine.ScanText(input, "JDG 3:29", 7, 3, 29, 0, adjustSandhi: false);

            Assert.Single(proposals);
            Assert.Equal("பதினாயிரம்", proposals[0].MatchedToken);
            Assert.Equal("10,000", proposals[0].Replacement);
            Assert.Equal(ConversionRuleType.TextToDigit, proposals[0].RuleType);

            string result = _engine.ApplyProposals(input, proposals);
            Assert.Equal("அக்காலத்திலே மோவாபியர்களில் ஏறக்குறையப் 10,000 பேரை வெட்டினார்கள்", result);
        }
    }
}
