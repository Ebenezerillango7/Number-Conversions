using NumberConversions.Core;
using Xunit;

namespace NumberConversions.Tests
{
    public class TamilTextToNumberTests
    {
        [Theory]
        [InlineData("இருநூறு", "200")]
        [InlineData("இருநூற்று ஐம்பது", "250")]
        [InlineData("ஐந்நூறு", "500")]
        [InlineData("ஆயிரம்", "1,000")]
        [InlineData("ஓராயிரம்", "1,000")]
        [InlineData("இரண்டாயிரம்", "2,000")]
        [InlineData("ஐயாயிரம்", "5,000")]
        [InlineData("பத்தாயிரம்", "10,000")]
        [InlineData("ஒரு லட்சம்", "1,00,000")]
        [InlineData("ஒரு லட்சத்து நாற்பத்து நான்காயிரம்", "1,44,000")]
        [InlineData("எட்டு லட்சம்", "8,00,000")]
        [InlineData("கோடி", "1,00,00,000")]
        public void TextToDigits_Converts200AndAbove_WithIndianGrouping(string tamilText, string expectedDigits)
        {
            bool success = TamilTextToNumberConverter.TryConvertPhraseToDigits(tamilText, out string formatted);
            Assert.True(success, $"Failed to parse: {tamilText}");
            Assert.Equal(expectedDigits, formatted);
        }

        [Fact]
        public void TextToDigits_OrdinalsAbove200()
        {
            bool success = TamilTextToNumberConverter.TryConvertPhraseToDigits("இருநூறாம்", out string formatted);
            Assert.True(success);
            Assert.Equal("200-ஆம்", formatted);
        }

        [Theory]
        [InlineData("நூறு")]               // 100 (< 200)
        [InlineData("நூற்று எழுபத்தைந்து")]  // 175 (< 200)
        [InlineData("இருபது")]             // 20 (< 200)
        [InlineData("ஐந்து")]              // 5 (< 200)
        public void TextToDigits_Under200_MustNotConvertToDigits(string tamilTextUnder200)
        {
            // Rule 5: Only >= 200 converts from text to digits.
            // Under 200 must remain as words.
            bool converted = TamilTextToNumberConverter.TryConvertPhraseToDigits(tamilTextUnder200, out _);
            Assert.False(converted, $"Values under 200 should NOT convert to digits: {tamilTextUnder200}");
        }

        [Theory]
        [InlineData(200, "200")]
        [InlineData(1000, "1,000")]
        [InlineData(12345, "12,345")]
        [InlineData(144000, "1,44,000")]
        [InlineData(1234567, "12,34,567")]
        [InlineData(12345678, "1,23,45,678")]
        [InlineData(123456789, "12,34,56,789")]
        public void IndianNumberFormatter_FollowsIndianGroupingConvention(long value, string expected)
        {
            string formatted = IndianNumberFormatter.Format(value);
            Assert.Equal(expected, formatted);
        }
    }
}
