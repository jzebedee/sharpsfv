using SharpSfv.Crc32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static IEnumerable<object[]> Encodings
        {
            get
            {
                //should default to UTF8
                yield return new object[] { null };
                yield return new object[] { Encoding.UTF8 };
                yield return new object[] { Encoding.ASCII };

            }
        }

        private static SfvBuilder GetBuilder(Stream outputStream, Encoding encoding)
        {
            SfvBuilder builder;
            if (encoding is null)
            {
                builder = new SfvBuilder(outputStream);
            }
            else
            {
                builder = new SfvBuilder(outputStream, encoding);
            }
            return builder;
        }

        [Theory]
        [MemberData(nameof(Encodings))]
        public void BuildInMemorySfvTest(Encoding encoding)
        {
            const string key = "Þórsteinn";
            var outputStream = new MemoryStream();

            const string comment = "Hello, world!";
            var builder = GetBuilder(outputStream, encoding);
            builder.AddComment(comment);
            
            var randomBuffer = GetRandomBuffer(0x1000);
            builder.AddStream(key, new MemoryStream(randomBuffer));

            const string dummyKey = "dummy.file";
            const uint dummyCrc = 0xDEADBEEF;
            builder.AddEntry(dummyKey, dummyCrc);

            encoding = encoding ?? Encoding.UTF8;
            var expected = encoding.GetBytes($"; {comment}{Environment.NewLine}"
                + $"{key} {(new Crc32Hash().Compute(randomBuffer)):X8}{Environment.NewLine}"
                + $"{dummyKey} {dummyCrc:X8}{Environment.NewLine}");
            var actual = outputStream.ToArray();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(Encodings))]
        public void BuildFileSfvTest(Encoding encoding)
        {
            const string key = "Þórsteinn";
            string filepath = Path.ChangeExtension(Path.GetTempFileName(), $"{key}.tmp");
            string filename = Path.GetFileName(filepath);

            var randomBuffer = GetRandomBuffer(0x1000);
            File.WriteAllBytes(filepath, randomBuffer);

            string sfvFilepath = Path.ChangeExtension(Path.GetTempFileName(), ".sfv.tmp");
            using (var outputStream = File.Create(sfvFilepath))
            {
                var builder = GetBuilder(outputStream, encoding);
                builder.AddFile(filepath);
            }
            encoding = encoding ?? Encoding.UTF8;

            var expected = encoding.GetBytes($"{filename} {(new Crc32Hash().Compute(randomBuffer)):X8}{Environment.NewLine}");
            var actual = File.ReadAllBytes(sfvFilepath);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(Encodings))]
        public void BuildDirectorySfvTest(Encoding encoding)
        {
            const string key = "Þórsteinn";

            string dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);

            var testFiles = Enumerable.Range(0, 10).Select(i => new
            {
                filename = key + i,
                content = GetRandomBuffer(0x1000)
            }).Select(a => new
            {
                path = Path.Combine(dir, a.filename),
                a.content,
                expected = $"{a.filename} {(new Crc32Hash().Compute(a.content)):X8}{Environment.NewLine}"
            });

            string expectedStr = "";
            foreach (var a in testFiles)
            {
                expectedStr += a.expected;
                File.WriteAllBytes(a.path, a.content);
            }

            string sfvFilepath = Path.ChangeExtension(Path.GetTempFileName(), ".sfv.tmp");
            using (var outputStream = File.Create(sfvFilepath))
            {
                var builder = GetBuilder(outputStream, encoding);
                builder.AddDirectory(dir);
            }
            encoding = encoding ?? Encoding.UTF8;

            var expected = encoding.GetBytes(expectedStr);
            var actual = File.ReadAllBytes(sfvFilepath);
            Assert.Equal(expected, actual);
        }
    }
}
