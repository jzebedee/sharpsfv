using System;
namespace SharpSfv.Crc32
{
    public struct Crc32Hash
    {
        private uint _currentCrc;

        public uint Hash => _currentCrc;

        public void Append(ReadOnlySpan<byte> input) => _currentCrc = Append(_currentCrc, input);

        public uint Compute(ReadOnlySpan<byte> input) => _currentCrc = Append(0, input);

        public static uint Append(uint crc, ReadOnlySpan<byte> input)
        {
            if (input.IsEmpty) return crc;

            uint crcLocal = uint.MaxValue ^ crc;

            ReadOnlySpan<uint> table = Crc32Table.Table;
            while (input.Length >= 16)
            {
                var a = table[(3 * 256) + input[12]]
                    ^ table[(2 * 256) + input[13]]
                    ^ table[(1 * 256) + input[14]]
                    ^ table[(0 * 256) + input[15]];

                var b = table[(7 * 256) + input[8]]
                    ^ table[(6 * 256) + input[9]]
                    ^ table[(5 * 256) + input[10]]
                    ^ table[(4 * 256) + input[11]];

                var c = table[(11 * 256) + input[4]]
                    ^ table[(10 * 256) + input[5]]
                    ^ table[(9 * 256) + input[6]]
                    ^ table[(8 * 256) + input[7]];

                var d = table[(15 * 256) + ((byte)crcLocal ^ input[0])]
                    ^ table[(14 * 256) + ((byte)(crcLocal >> 8) ^ input[1])]
                    ^ table[(13 * 256) + ((byte)(crcLocal >> 16) ^ input[2])]
                    ^ table[(12 * 256) + ((byte)(crcLocal >> 24) ^ input[3])];

                crcLocal = d ^ c ^ b ^ a;
                input = input.Slice(16);
            }

            while (!input.IsEmpty)
            {
                crcLocal = table[(byte)(crcLocal ^ input[0])] ^ crcLocal >> 8;
                input = input.Slice(1);
            }

            return crcLocal ^ uint.MaxValue;
        }
    }
}
