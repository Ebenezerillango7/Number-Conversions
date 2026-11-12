using System;
using NumberConversions.Core;
using Xunit;

namespace NumberConversions.Tests
{
    public class TamilCardinalTests
    {
        [Fact]
        public void ZeroToNine_ConvertsCorrectly()
        {
            Assert.Equal("பூஜ்யம்", TamilCardinalToTextConverter.Convert(0));
            Assert.Equal("ஒன்று", TamilCardinalToTextConverter.Convert(1));
            Assert.Equal("இரண்டு", TamilCardinalToTextConverter.Convert(2));
            Assert.Equal("மூன்று", TamilCardinalToTextConverter.Convert(3));
            Assert.Equal("நான்கு", TamilCardinalToTextConverter.Convert(4));
            Assert.Equal("ஐந்து", TamilCardinalToTextConverter.Convert(5));
            Assert.Equal("ஆறு", TamilCardinalToTextConverter.Convert(6));
            Assert.Equal("ஏழு", TamilCardinalToTextConverter.Convert(7));
            Assert.Equal("எட்டு", TamilCardinalToTextConverter.Convert(8));
            Assert.Equal("ஒன்பது", TamilCardinalToTextConverter.Convert(9));
        }

        [Fact]
        public void Teens_ConvertsCorrectly()
        {
            Assert.Equal("பத்து", TamilCardinalToTextConverter.Convert(10));
            Assert.Equal("பதினொன்று", TamilCardinalToTextConverter.Convert(11));
            Assert.Equal("பன்னிரண்டு", TamilCardinalToTextConverter.Convert(12));
            Assert.Equal("பதின்மூன்று", TamilCardinalToTextConverter.Convert(13));
            Assert.Equal("பதினான்கு", TamilCardinalToTextConverter.Convert(14));
            Assert.Equal("பதினைந்து", TamilCardinalToTextConverter.Convert(15));
            Assert.Equal("பதினாறு", TamilCardinalToTextConverter.Convert(16));
            Assert.Equal("பதினேழு", TamilCardinalToTextConverter.Convert(17));
            Assert.Equal("பதினெட்டு", TamilCardinalToTextConverter.Convert(18));
            Assert.Equal("பத்தொன்பது", TamilCardinalToTextConverter.Convert(19));
        }

        [Fact]
        public void Tens_ConvertsCorrectly()
        {
            Assert.Equal("இருபது", TamilCardinalToTextConverter.Convert(20));
            Assert.Equal("முப்பது", TamilCardinalToTextConverter.Convert(30));
            Assert.Equal("நாற்பது", TamilCardinalToTextConverter.Convert(40));
            Assert.Equal("ஐம்பது", TamilCardinalToTextConverter.Convert(50));
            Assert.Equal("அறுபது", TamilCardinalToTextConverter.Convert(60));
            Assert.Equal("எழுபது", TamilCardinalToTextConverter.Convert(70));
            Assert.Equal("எண்பது", TamilCardinalToTextConverter.Convert(80));
            Assert.Equal("தொண்ணூறு", TamilCardinalToTextConverter.Convert(90));
        }

        [Fact]
        public void OneHundredAndSeventyFive_MatchesUserSpecification()
        {
            // Specifically specified in user prompt: "One hundred and seventy five instead of 175"
            string result = TamilCardinalToTextConverter.Convert(175);
            Assert.Equal("நூற்று எழுபத்தைந்து", result);
        }

        [Fact]
        public void BoundaryRange_Validations()
        {
            Assert.Equal("நூறு", TamilCardinalToTextConverter.Convert(100));
            Assert.Equal("நூற்று தொண்ணூற்றொன்பது", TamilCardinalToTextConverter.Convert(199));
            Assert.Throws<ArgumentOutOfRangeException>(() => TamilCardinalToTextConverter.Convert(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => TamilCardinalToTextConverter.Convert(200));
        }
    }
}
