using SharpSfv.Crc32;
using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace SharpSfv
{
    public sealed class SfvBuilder
    {
        private readonly IBufferWriter<byte> _writer;
        private readonly Encoding _encoding;

        public SfvBuilder(IBufferWriter<byte> writer, Encoding encoding)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));

            _writer = writer;
            _encoding = encoding;
        }
        public SfvBuilder(IBufferWriter<byte> writer) : this(writer, Encoding.UTF8) { }

        private void WriteLine(string key, uint crc32)
        {
            var lineBytes = _encoding.GetBytes($"{key} {crc32:X8}{Environment.NewLine}");
            _writer.Write(lineBytes);
        }

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
