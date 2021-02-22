using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;

namespace SharpSfv.Tests
{
    public class SfvBuilderTests
    {
        private static readonly Random _rand = new Random();
        private static byte[] GetRandomBuffer(int length)
        {
            var input = new byte[length];
            _rand.NextBytes(input);
            return input;
        }

        [Fact]
        public void BuildInMemorySfvTest()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void BuildFileSfvTest()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void BuildDirectorySfvTest()
        {
            throw new NotImplementedException();
        }
    }
}
