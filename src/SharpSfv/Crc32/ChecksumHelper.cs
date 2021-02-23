using System;
using System.Buffers;
using System.IO;

namespace SharpSfv.Crc32
{
    internal static class ChecksumHelper
    {
        private const int SmallBufferSize = 0x1000;
        private const int LargeBufferThreshold = 0x10000;
        private const int LargeBufferSize = 0x10000;

        public static uint FromStream(Stream stream)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Stream must be readable", nameof(stream));

            var crc = new Crc32Hash();

            var length = stream.Length;
            int initialSize = length == 0 ? SmallBufferSize : length > LargeBufferThreshold ? LargeBufferSize : (int)length;

            byte[] buffer = ArrayPool<byte>.Shared.Rent(initialSize);
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

            return crc.Hash;
        }

        public static uint FromFile(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return FromStream(stream);
            }
        }
    }
}
