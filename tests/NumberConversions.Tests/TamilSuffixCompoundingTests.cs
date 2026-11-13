using NumberConversions.Core;
using Xunit;

namespace NumberConversions.Tests
{
    public class TamilSuffixCompoundingTests
    {
        private readonly TamilNumberConversionEngine _engine = new TamilNumberConversionEngine();

        [Fact]
        public void Genesis14_4_OrdinalWithSpace_ConvertsToFusedOrdinalWithoutOrphanSuffix()
        {
            // Genesis 14:4: "இவர்கள் 12 ஆண்டுகள் கெதர்லாகோமேரைச் சேவித்து, 13 ஆம் வருடத்திலே கலகம் செய்தார்கள்."
            string verse = "இவர்கள் 12 ஆண்டுகள் கெதர்லாகோமேரைச் சேவித்து, 13 ஆம் வருடத்திலே கலகம் செய்தார்கள்.";
            var proposals = _engine.ScanText(verse, "GEN 14:4");

            Assert.Equal(2, proposals.Count);

            // First proposal: 12 -> பன்னிரண்டு
            Assert.Equal("12", proposals[0].MatchedToken);
            Assert.Equal("பன்னிரண்டு", proposals[0].Replacement);

            // Second proposal: 13 ஆம் -> பதின்மூன்றாம் (fused, no orphan "ஆம்")
            Assert.Equal("13 ஆம்", proposals[1].MatchedToken);
            Assert.Equal("பதின்மூன்றாம்", proposals[1].Replacement);

            string result = _engine.ApplyProposals(verse, proposals);
            Assert.Equal("இவர்கள் பன்னிரண்டு ஆண்டுகள் கெதர்லாகோமேரைச் சேவித்து, பதின்மூன்றாம் வருடத்திலே கலகம் செய்தார்கள்.", result);
            Assert.DoesNotContain("பதின்மூன்று ஆம்", result);
        }

        [Fact]
        public void Genesis14_5_OrdinalWithSpace_ConvertsToFusedOrdinalWithoutOrphanSuffix()
        {
            // Genesis 14:5: "14 ஆம் வருடத்திலே கெதர்லாகோமேரும்..."
            string verse = "14 ஆம் வருடத்திலே கெதர்லாகோமேரும், அவனோடு கூடியிருந்த ராஜாக்களும் வந்து";
            var proposals = _engine.ScanText(verse, "GEN 14:5");

            Assert.Single(proposals);
            Assert.Equal("14 ஆம்", proposals[0].MatchedToken);
            Assert.Equal("பதினான்காம்", proposals[0].Replacement);

            string result = _engine.ApplyProposals(verse, proposals);
            Assert.StartsWith("பதினான்காம் வருடத்திலே", result);
            Assert.DoesNotContain("பதினான்கு ஆம்", result);
        }

        [Theory]
        [InlineData("13 ஆம்", "பதின்மூன்றாம்")]
        [InlineData("13ஆம்", "பதின்மூன்றாம்")]
        [InlineData("13-ஆம்", "பதின்மூன்றாம்")]
        [InlineData("13 - ஆம்", "பதின்மூன்றாம்")]
        [InlineData("13 ம்", "பதின்மூன்றாம்")]
        [InlineData("13ம்", "பதின்மூன்றாம்")]
        [InlineData("13 ஆவது", "பதின்மூன்றாவது")]
        [InlineData("13ஆவது", "பதின்மூன்றாவது")]
        [InlineData("13-ஆவது", "பதின்மூன்றாவது")]
        [InlineData("13 வது", "பதின்மூன்றாவது")]
        [InlineData("13வது", "பதின்மூன்றாவது")]
        [InlineData("13-வது", "பதின்மூன்றாவது")]
        [InlineData("1 ஆம்", "முதலாம்")]
        [InlineData("1 ஆவது", "முதலாவது")]
        [InlineData("2 ஆம்", "இரண்டாம்")]
        [InlineData("3 ஆம்", "மூன்றாம்")]
        [InlineData("7 ஆம்", "ஏழாம்")]
        [InlineData("10 ஆம்", "பத்தாம்")]
        [InlineData("20 ஆம்", "இருபதாம்")]
        [InlineData("100 ஆம்", "நூறாம்")]
        public void OrdinalFormats_AllVariationsConvertCleanly(string input, string expected)
        {
            string text = $"அவன் {input} நாளில் வந்தான்.";
            var proposals = _engine.ScanText(text);

            Assert.Single(proposals);
            Assert.Equal(input, proposals[0].MatchedToken);
            Assert.Equal(expected, proposals[0].Replacement);

            string result = _engine.ApplyProposals(text, proposals);
            Assert.Equal($"அவன் {expected} நாளில் வந்தான்.", result);
        }

        [Theory]
        [InlineData("13 இல்", "பதின்மூன்றில்")]
        [InlineData("13-இல்", "பதின்மூன்றில்")]
        [InlineData("13 க்கு", "பதின்மூன்றுக்கு")]
        [InlineData("13-க்கு", "பதின்மூன்றுக்கு")]
        [InlineData("13 இலிருந்து", "பதின்மூன்றிலிருந்து")]
        [InlineData("13-லிருந்து", "பதின்மூன்றிலிருந்து")]
        [InlineData("13 ஐ", "பதின்மூன்றை")]
        [InlineData("13-ஐ", "பதின்மூன்றை")]
        [InlineData("100 இல்", "நூற்றில்")]
        [InlineData("100 க்கு", "நூற்றுக்கு")]
        [InlineData("100 இலிருந்து", "நூற்றிலிருந்து")]
        [InlineData("100 ஐ", "நூறை")]
        public void CaseEndings_CompoundedAccurately(string input, string expected)
        {
            string text = $"அவன் {input} பார்த்தான்.";
            var proposals = _engine.ScanText(text);

            Assert.Single(proposals);
            Assert.Equal(input, proposals[0].MatchedToken);
            Assert.Equal(expected, proposals[0].Replacement);

            string result = _engine.ApplyProposals(text, proposals);
            Assert.Equal($"அவன் {expected} பார்த்தான்.", result);
        }

        [Fact]
        public void CountNouns_AreNotMistakenForSuffixes()
        {
            // "13 முறை" -> 13 alone should convert to பதின்மூன்று, leaving முறை intact
            string text = "அவன் 13 முறை சொன்னான்.";
            var proposals = _engine.ScanText(text);

            Assert.Single(proposals);
            Assert.Equal("13", proposals[0].MatchedToken);
            Assert.Equal("பதின்மூன்று", proposals[0].Replacement);

            string result = _engine.ApplyProposals(text, proposals);
            Assert.Equal("அவன் பதின்மூன்று முறை சொன்னான்.", result);
        }

        [Fact]
        public void TextToDigits_OrdinalsAbove200_ConvertWithOrdinalSuffix()
        {
            // இருநூறாம் -> 200-ஆம்
            string text1 = "அவன் இருநூறாம் வருஷத்தில் வந்தான்.";
            var p1 = _engine.ScanText(text1);
            Assert.Single(p1);
            Assert.Equal("இருநூறாம்", p1[0].MatchedToken);
            Assert.Equal("200-ஆம்", p1[0].Replacement);

            // இருநூறு ஆம் -> 200-ஆம்
            string text2 = "அவன் இருநூறு ஆம் வருஷத்தில் வந்தான்.";
            var p2 = _engine.ScanText(text2);
            Assert.Single(p2);
            Assert.Equal("இருநூறு ஆம்", p2[0].MatchedToken);
            Assert.Equal("200-ஆம்", p2[0].Replacement);

            // இருநூறாவது -> 200-வது
            string text3 = "அவன் இருநூறாவது வருஷத்தில் வந்தான்.";
            var p3 = _engine.ScanText(text3);
            Assert.Single(p3);
            Assert.Equal("இருநூறாவது", p3[0].MatchedToken);
            Assert.Equal("200-ஆம்", p3[0].Replacement);
        }
    }
}
