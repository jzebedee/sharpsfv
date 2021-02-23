using SharpSfv.Crc32;
using System;
using Xunit;

namespace SharpSfv.Tests
{
    public class Crc32Tests
    {
        private static readonly Random _rand = new Random();
        private static byte[] GetRandomBuffer(int length)
        {
            var input = new byte[length];
            _rand.NextBytes(input);
            return input;
        }

        private uint NaiveCrc32(ReadOnlySpan<byte> input, uint poly = 0xEDB88320)
        {
            uint crc = unchecked((uint)-1);

            while (!input.IsEmpty)
            {
                crc ^= input[0];
                input = input.Slice(1);
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & 1) == 1) crc = (crc >> 1) ^ poly;
                    else crc >>= 1;
                }
            }
            return ~crc;
        }

        [Fact]
        public void Crc32ComputeTest()
        {
            var testBuffer = GetRandomBuffer(0x1000);

            var naiveCrc32 = NaiveCrc32(testBuffer);
            var sfvCrc32 = new Crc32Hash().Compute(testBuffer);

            Assert.Equal(naiveCrc32, sfvCrc32);
        }

        [Fact]
        public void Crc32AppendTest()
        {
            Span<byte> testBuffer = GetRandomBuffer(0x1000);

            var naiveCrc32 = NaiveCrc32(testBuffer);
            var sfvCrc32 = new Crc32Hash();
            while (!testBuffer.IsEmpty)
            {
                var chunk = testBuffer.Slice(0, 0x100);
                sfvCrc32.Append(chunk);
                testBuffer = testBuffer.Slice(0x100);
            }

            Assert.Equal(naiveCrc32, sfvCrc32.Hash);
        }
    }
}
