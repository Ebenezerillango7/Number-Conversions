using NumberConversions.Core;
using Xunit;

namespace NumberConversions.Tests
{
    public class TamilOrdinalTests
    {
        [Theory]
        [InlineData("2nd", 2)]
        [InlineData("2-ஆம்", 2)]
        [InlineData("2ஆம்", 2)]
        [InlineData("2வது", 2)]
        [InlineData("2-வது", 2)]
        [InlineData("2ம்", 2)]
        [InlineData("1st", 1)]
        [InlineData("1-ஆம்", 1)]
        [InlineData("3rd", 3)]
        [InlineData("175-ஆம்", 175)]
        public void MatchOrdinal_DetectsNumberAndSuffix(string input, int expectedNumber)
        {
            bool matched = TamilOrdinalToTextConverter.TryMatchOrdinal(input, out int number, out string suffix);
            Assert.True(matched);
            Assert.Equal(expectedNumber, number);
        }

        [Fact]
        public void SecondInsteadOf2nd_MatchesUserSpecification()
        {
            // Specifically requested in user prompt: "Second instead of 2nd"
            string result = TamilOrdinalToTextConverter.Convert(2, OrdinalStyle.Aam);
            Assert.Equal("இரண்டாம்", result);

            string resultAavathu = TamilOrdinalToTextConverter.Convert(2, OrdinalStyle.Aavathu);
            Assert.Equal("இரண்டாவது", resultAavathu);
        }

        [Fact]
        public void FirstAndThirdOrdinals()
        {
            Assert.Equal("முதலாம்", TamilOrdinalToTextConverter.Convert(1));
            Assert.Equal("மூன்றாம்", TamilOrdinalToTextConverter.Convert(3));
            Assert.Equal("பத்தாம்", TamilOrdinalToTextConverter.Convert(10));
        }

        [Fact]
        public void CompoundOrdinal_175th()
        {
            Assert.Equal("நூற்று எழுபத்தைந்தாம்", TamilOrdinalToTextConverter.Convert(175));
        }
    }
}
