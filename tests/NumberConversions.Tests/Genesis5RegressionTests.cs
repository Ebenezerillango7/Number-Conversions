using System.Collections.Generic;
using System.Linq;
using NumberConversions.Core;
using Xunit;

namespace NumberConversions.Tests
{
    public class Genesis5RegressionTests
    {
        [Fact]
        public void Genesis5_Scenario_DoesNotDuplicateOrCorruptAdjacentText()
        {
            var engine = new TamilNumberConversionEngine();

            // Verses straight from user's Genesis 5 screenshot
            string verse15 = "மகலாலெயேல் 65 வயதானபோது, யாரேதைப் பெற்றெடுத்தான்.";
            string verse18 = "யாரேத் 162 வயதானபோது, ஏனோக்கைப் பெற்றெடுத்தான்.";
            string verse21 = "ஏனோக்கு 65 வயதானபோது, மெத்தூசலாவைப் பெற்றெடுத்தான்.";
            string verse23 = "ஏனோக்கு உயிரோடிருந்த நாட்களெல்லாம் 365 ஆண்டுகள்.";
            string verse25 = "மெத்தூசலா 187 வயதானபோது, லாமேக்கைப் பெற்றெடுத்தான்.";

            // 1. Scan verse 15
            var p15 = engine.ScanText(verse15, "GEN 5:15", 1, 5, 15, tokenIndex: 2);
            Assert.Single(p15);
            Assert.Equal("65", p15[0].MatchedToken);
            Assert.Equal("அறுபத்தைந்து", p15[0].Replacement);

            // 2. Scan verse 18
            var p18 = engine.ScanText(verse18, "GEN 5:18", 1, 5, 18, tokenIndex: 5);
            Assert.Single(p18);
            Assert.Equal("162", p18[0].MatchedToken);
            Assert.Equal("நூற்று அறுபத்திரண்டு", p18[0].Replacement);

            // 3. Scan verse 21
            var p21 = engine.ScanText(verse21, "GEN 5:21", 1, 5, 21, tokenIndex: 8);
            Assert.Single(p21);
            Assert.Equal("65", p21[0].MatchedToken);
            Assert.Equal("அறுபத்தைந்து", p21[0].Replacement);

            // 4. Scan verse 23 (Contains 365)
            // 365 is >= 200 and already digits, so it must produce ZERO proposals!
            var p23 = engine.ScanText(verse23, "GEN 5:23", 1, 5, 23, tokenIndex: 10);
            Assert.Empty(p23);

            // 5. Scan verse 25
            var p25 = engine.ScanText(verse25, "GEN 5:25", 1, 5, 25, tokenIndex: 12);
            Assert.Single(p25);
            Assert.Equal("187", p25[0].MatchedToken);
            Assert.Equal("நூற்று எண்பத்தேழு", p25[0].Replacement);

            // Apply conversions to each verse using strictly its own token proposals
            string v15Applied = engine.ApplyProposals(verse15, p15);
            Assert.Equal("மகலாலெயேல் அறுபத்தைந்து வயதானபோது, யாரேதைப் பெற்றெடுத்தான்.", v15Applied);

            string v18Applied = engine.ApplyProposals(verse18, p18);
            Assert.Equal("யாரேத் நூற்று அறுபத்திரண்டு வயதானபோது, ஏனோக்கைப் பெற்றெடுத்தான்.", v18Applied);

            string v21Applied = engine.ApplyProposals(verse21, p21);
            Assert.Equal("ஏனோக்கு அறுபத்தைந்து வயதானபோது, மெத்தூசலாவைப் பெற்றெடுத்தான்.", v21Applied);

            string v23Applied = engine.ApplyProposals(verse23, p23);
            Assert.Equal("ஏனோக்கு உயிரோடிருந்த நாட்களெல்லாம் 365 ஆண்டுகள்.", v23Applied);

            string v25Applied = engine.ApplyProposals(verse25, p25);
            Assert.Equal("மெத்தூசலா நூற்று எண்பத்தேழு வயதானபோது, லாமேக்கைப் பெற்றெடுத்தான்.", v25Applied);
        }

        [Fact]
        public void CrossVersePollution_IsPreventedByExactIndexVerification()
        {
            var engine = new TamilNumberConversionEngine();

            string verse15 = "மகலாலெயேல் 65 வயதானபோது, யாரேதைப் பெற்றெடுத்தான்.";
            string verse21 = "ஏனோக்கு 65 வயதானபோது, மெத்தூசலாவைப் பெற்றெடுத்தான்.";
            string verse23 = "ஏனோக்கு உயிரோடிருந்த நாட்களெல்லாம் 365 ஆண்டுகள்.";

            var p15 = engine.ScanText(verse15, "GEN 5:15", 1, 5, 15, tokenIndex: 2);
            var p21 = engine.ScanText(verse21, "GEN 5:21", 1, 5, 21, tokenIndex: 8);

            // Even if someone erroneously combines proposals from verse 15 and verse 21:
            var mixedProposals = new List<ConversionProposal>();
            mixedProposals.AddRange(p15);
            mixedProposals.AddRange(p21);

            // When applied to Verse 15:
            // The proposal from Verse 21 has StartIndex=8 (which in Verse 15 does NOT match "65").
            // Safety invariant in ApplyProposals MUST reject Verse 21's proposal and ONLY apply Verse 15's!
            string result15 = engine.ApplyProposals(verse15, mixedProposals);
            Assert.Equal("மகலாலெயேல் அறுபத்தைந்து வயதானபோது, யாரேதைப் பெற்றெடுத்தான்.", result15);

            // When applied to Verse 21:
            // The proposal from Verse 15 has StartIndex=12 (which in Verse 21 does NOT match "65").
            // Safety invariant in ApplyProposals MUST reject Verse 15's proposal and ONLY apply Verse 21's!
            string result21 = engine.ApplyProposals(verse21, mixedProposals);
            Assert.Equal("ஏனோக்கு அறுபத்தைந்து வயதானபோது, மெத்தூசலாவைப் பெற்றெடுத்தான்.", result21);

            // When applied to Verse 23:
            // Neither proposal matches at its respective StartIndex, so Verse 23 remains completely unchanged!
            string result23 = engine.ApplyProposals(verse23, mixedProposals);
            Assert.Equal(verse23, result23);
        }

        [Fact]
        public void MultipleIdenticalNumbersInSameVerse_AreBothConvertedCleanly()
        {
            var engine = new TamilNumberConversionEngine();
            string text = "அவன் 12 ஆடுகளையும் 12 மாடுகளையும் கொடுத்தான்.";

            var proposals = engine.ScanText(text);
            Assert.Equal(2, proposals.Count);
            Assert.Equal("12", proposals[0].MatchedToken);
            Assert.Equal("12", proposals[1].MatchedToken);

            string result = engine.ApplyProposals(text, proposals);
            Assert.Equal("அவன் பன்னிரண்டு ஆடுகளையும் பன்னிரண்டு மாடுகளையும் கொடுத்தான்.", result);
        }
    }
}
