using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpSfv
{
    public sealed class SfvParser
    {
        public static SfvParser FromFile(string path) => FromBuffer(File.ReadAllBytes(path));
        public static SfvParser FromFile(string path, Encoding encoding) => FromLines(File.ReadLines(path, encoding));
        public static SfvParser FromStream(Stream stream)
        {
            using (var ms = new MemoryStream())
            using (stream)
            {
                stream.CopyTo(ms);
                if (!ms.TryGetBuffer(out var segment))
                {
                    throw new InvalidOperationException("failed to buffer stream");
                }

                return FromBuffer(segment);
            }
        }
        public static SfvParser FromStream(Stream stream, Encoding encoding)
        {
            var parser = new SfvParser();

            using (stream)
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    parser.ParseLine(line);
                }
            }

            return parser;
        }
        public static SfvParser FromLines(IEnumerable<string> lines)
        {
            var parser = new SfvParser();

            foreach (var line in lines)
            {
                parser.ParseLine(line);
            }

            return parser;
        }
        public static SfvParser FromBuffer(ReadOnlySpan<byte> utf8Buffer)
        {
            var parser = new SfvParser();

            var newlines = LineEndings.AsSpan();
            int i;
            while ((i = utf8Buffer.IndexOfAny(newlines)) != -1)
            {
                ReadOnlySpan<byte> line = utf8Buffer.Slice(0, i);
                parser.ParseLine(line);
                utf8Buffer = utf8Buffer.Slice(i + 1);
            }

            if(!utf8Buffer.IsEmpty)
            {
                parser.ParseLine(utf8Buffer);
            }

            return parser;
        }

        public SfvParser()
        {
        }

        private const byte CommentPrefix = (byte)';';
        private const byte EntrySeparator = (byte)' ';

        private static readonly byte[] LineEndings = Environment.NewLine.ToUtf8();

        private readonly List<SfvLine> _entries = new List<SfvLine>();
        
        public IReadOnlyList<SfvLine> Entries => _entries;

        public void ParseLine(string line) => ParseLine(line.ToUtf8());

        public void ParseLine(ReadOnlySpan<byte> utf8line)
        {
            if (utf8line.IsEmpty)
            {
                return;
            }

            if (utf8line[0] == CommentPrefix)
            {
                HandleComment(utf8line.Slice(1));
                return;
            }

            var separator = utf8line.IndexOf(EntrySeparator);
            if (separator == -1)
            {
                throw new InvalidOperationException("bad format");
            }

            var key = utf8line.Slice(0, separator);
            if (!Utf8Parser.TryParse(utf8line.Slice(separator + 1), out uint crc32, out _, standardFormat: 'x'))
            {
                throw new InvalidOperationException("bad format");
            }

            HandleEntry(key, crc32);
        }

        private void HandleComment(ReadOnlySpan<byte> span)
        {
            string commentString;
#if NET
            commentString = Encoding.UTF8.GetString(span);
#else //slow path with copy
            commentString = Encoding.UTF8.GetString(span.ToArray());
#endif
            _entries.Add(new SfvLine(SfvLineType.Comment, commentString, default));
        }

        private void HandleEntry(ReadOnlySpan<byte> span, uint crc32)
        {
            string keyString;
#if NET
            keyString = Encoding.UTF8.GetString(span);
#else //slow path with copy
            keyString = Encoding.UTF8.GetString(span.ToArray());
#endif
            _entries.Add(new SfvLine(SfvLineType.Key, keyString, crc32));
        }
    }
}
