using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpSfv
{
    public sealed class SfvParser
    {
        //public static SfvParser FromFile(string path)
        //{

        //}
        //public static SfvParser FromFile(string path, Encoding encoding)
        //{

        //}
        //public static SfvParser FromStream(Stream stream)
        //{

        //}
        //public static SfvParser FromStream(Stream stream, Encoding encoding)
        //{

        //}
        //public static SfvParser FromLines(IEnumerable<string> lines)
        //{

        //}
        //public static SfvParser FromBuffer(ReadOnlySpan<byte> utf8Buffer)
        //{

        //}

        //private SfvParser()
        //{
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<byte> ToUtf8(string str) => Encoding.UTF8.GetBytes(str);

        public SfvParser(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                ParseLine(ToUtf8(line));
            }
        }

        public SfvParser(Stream stream, Encoding encoding = null)
        {
            using (stream)
            using (var reader = new StreamReader(stream, encoding ?? Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    ParseLine(ToUtf8(line));
                }
            }
        }
        public SfvParser(string path, Encoding encoding = null)
        {
            var lines = File.ReadLines(path, encoding ?? Encoding.UTF8);
            foreach (var line in lines)
            {
                ParseLine(ToUtf8(line));
            }
        }
        public SfvParser()
        {
        }

        private const byte CommentPrefix = (byte)';';
        private const byte EntrySeparator = (byte)' ';

        public void ParseLine(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
            {
                return;
            }

            if (span[0] == CommentPrefix)
            {
                HandleComment(span.Slice(1));
                return;
            }

            var separator = span.IndexOf(EntrySeparator);
            if (separator == -1)
            {
                throw new InvalidOperationException("bad format");
            }

            var key = span.Slice(0, separator);
            if (!Utf8Parser.TryParse(span.Slice(separator + 1), out uint crc32, out _, standardFormat: 'x'))
            {
                throw new InvalidOperationException("bad format");
            }

            HandleEntry(key, crc32);
        }

        private void HandleComment(ReadOnlySpan<byte> span)
        {
            throw new NotImplementedException();
            //Debug.WriteLine($"comment:{Encoding.UTF8.GetString(span.ToArray())}");
        }

        private void HandleEntry(ReadOnlySpan<byte> key, uint crc32)
        {
            throw new NotImplementedException();
            //Debug.WriteLine($"key:{Encoding.UTF8.GetString(key.ToArray())} crc32:{crc32:X8}");
        }
    }
}
