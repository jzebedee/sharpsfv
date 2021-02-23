using SharpSfv.Crc32;
using System;
using System.IO;
using System.Text;

namespace SharpSfv
{
    public sealed class SfvBuilder
    {
        private readonly Stream _stream;
        private readonly Encoding _encoding;

        public SfvBuilder(Stream stream, Encoding encoding)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            _stream = stream;
            _encoding = encoding;
        }
        public SfvBuilder(Stream stream) : this(stream, Encoding.UTF8) { }

        private void WriteLine(string key, uint crc32)
        {
            var lineBytes = _encoding.GetBytes($"{key} {crc32:X8}{Environment.NewLine}");
            _stream.Write(lineBytes, 0, lineBytes.Length);
        }

        public void AddComment(string comment)
        {
            var lineBytes = _encoding.GetBytes($"; {comment}{Environment.NewLine}");
            _stream.Write(lineBytes, 0, lineBytes.Length);
        }

        public void AddEntry(string key, uint crc32) => WriteLine(key, crc32);

        public void AddStream(string key, Stream stream) => WriteLine(key, ChecksumHelper.FromStream(stream));

        public void AddFile(string path) => WriteLine(Path.GetFileName(path), ChecksumHelper.FromFile(path));

        public void AddDirectory(string path)
        {
            foreach (var file in Directory.EnumerateFiles(path))
            {
                AddFile(file);
            }
        }
    }
}
