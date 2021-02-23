using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SharpSfv.Tests
{
    public class SfvParserTests
    {
        private const string GoodSfv0 = "test.txt DEADBEEF";

        private const string GoodSfv1 =
            "dummy.txt DEADBEEF\r\n" +
            "dummy1.txt BEEFDEED\r\n" +
            "; just taking a break\r\n" +
            "x DADADADA";

        private static readonly char[] LineEndings = Environment.NewLine.ToCharArray();

        [Theory]
        [InlineData(GoodSfv0, true)]
        [InlineData(GoodSfv1, true)]
        public void ParseSfvTest(string sfv, bool good)
        {
            var parser = new SfvParser();
            foreach(var line in sfv.Split(LineEndings))
            {
                ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(line);
                parser.ParseLine(span);
            }
        }
    }
}
