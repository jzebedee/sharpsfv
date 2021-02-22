using SharpSfv.Crc32;
using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace SharpSfv
{
    public sealed class SfvBuilder
    {
        private IBufferWriter<byte> _writer;
        private Encoding _encoding;

        public SfvBuilder(IBufferWriter<byte> writer, Encoding encoding = null)
        {
            _writer = writer;
            _encoding = encoding ?? Encoding.UTF8;
        }

        private void WriteLine(string key, uint crc32)
        {
            //slow path
            var lineBytes = _encoding.GetBytes($"{key} {crc32:x8}{Environment.NewLine}");
            _writer.Write(lineBytes);

            //var keyBytes = _encoding.GetByteCount(key);
            //Utf8Formatter.TryFormat(crc32, )
            //_writer.GetSpan(keyBytes + 1 )
        }

        public void AddStream(string key, Stream stream)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Stream must be readable", nameof(stream));

            var crc = new Crc32Hash();

            byte[] buffer = ArrayPool<byte>.Shared.Rent(0x1000);
            try
            {
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    crc.Append(buffer.AsSpan(0, bytesRead));
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            WriteLine(key, crc.Hash);
        }

        public void AddFile(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                AddStream(Path.GetFileName(path), stream);
            }
        }
    }
}
