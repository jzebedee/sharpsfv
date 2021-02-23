using System.Text;
using Xunit;

namespace SharpSfv.Tests
{
    public class SfvParserTests
    {
        //private static readonly char[] LineEndings = Environment.NewLine.ToCharArray();
        //
        //[Theory]
        //[InlineData(GoodSfv0, true)]
        //[InlineData(GoodSfv1, true)]
        //public void ParseSfvDoesntThrowTest(string sfv, bool good)
        //{
        //    var parser = new SfvParser();
        //    foreach(var line in sfv.Split(LineEndings))
        //    {
        //        ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(line);
        //        parser.ParseLine(span);
        //    }
        //}

        private const string GoodSingleLineSfv = "test.txt DEADBEEF";

        [Fact]
        public void ParseSingleLineSfvTest()
        {
            var parser = SfvParser.FromLines(new string[] { GoodSingleLineSfv });
            var entries = parser.Entries;
            Assert.NotNull(entries);
            Assert.NotEmpty(entries);

            var expected = new SfvLine(SfvLineType.Key, "test.txt", 0xDEADBEEF);
            Assert.Equal(expected, entries[0]);
        }

        private const string GoodMultiLineSfv =
           "dummy.txt DEADBEEF\r\n" +
           "dummy1.txt BEEFDEED\r\n" +
           "; just taking a break\r\n" +
           "x DADADADA";

        [Fact]
        public void ParseMultiLineSfvTest()
        {
            var parser = SfvParser.FromBuffer(Encoding.UTF8.GetBytes(GoodMultiLineSfv));
            var entries = parser.Entries;
            Assert.NotNull(entries);
            Assert.Equal(4, entries.Count);

            var expected1 = new SfvLine(SfvLineType.Key, "dummy.txt", 0xDEADBEEF);
            Assert.Equal(expected1, entries[0]);
 
            var expected2 = new SfvLine(SfvLineType.Key, "dummy1.txt", 0xBEEFDEED);
            Assert.Equal(expected2, entries[1]);

            var expected3 = new SfvLine(SfvLineType.Comment, " just taking a break", default);
            Assert.Equal(expected3, entries[2]);

            var expected4 = new SfvLine(SfvLineType.Key, "x", 0xDADADADA);
            Assert.Equal(expected4, entries[3]);
        }
    }
}
