using System;

namespace SharpSfv
{
    public readonly struct SfvLine
    {
        public readonly SfvLineType Type;

        public readonly string Entry;

        public readonly uint Crc32;

        public SfvLine(SfvLineType type, string entry, uint crc32)
        {
            Type = type;
            Entry = entry;
            Crc32 = crc32;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case SfvLineType.Key:
                    return $"{Entry} {Crc32:X8}";
                case SfvLineType.Comment:
                    return $";{Entry}";
                default:
                    throw new InvalidOperationException("Unknown line type");
            }
        }
    }
}
