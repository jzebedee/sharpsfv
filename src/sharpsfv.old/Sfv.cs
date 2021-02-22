using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace sharpsfv
{
    public class Sfv
    {
        private readonly IEnumerable<string> _lines;

        public Sfv(string path) : this(File.ReadLines(path))
        {
        }

        public Sfv(IEnumerable<string> lines)
        {
            _lines = lines;
        }

        public Sfv(Stream stream, Encoding encoding = null)
        {
            _lines = LinesFromStream(encoding != null ? new StreamReader(stream, encoding) : new StreamReader(stream));
        }

        private IEnumerable<string> LinesFromStream(StreamReader reader)
        {
            using (reader)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    yield return line;
            }
        }

        private KeyValuePair<string, uint> ParseLine(string line)
        {
            var groups = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (groups.Length == 2) {
                uint crc;
                if (uint.TryParse(groups[1], out crc))
                    return new KeyValuePair<string, uint>(groups[0], crc);
            }

            throw new ArgumentException("Line was in the wrong format", "line");
        }

        public IReadOnlyDictionary<string, uint> Entries
        {
            get
            {
                return new ReadOnlyDictionary<string, uint>(_lines.Select(ParseLine).ToDictionary(kvp=>kvp.Key, kvp=>kvp.Value));
            }
        }
    }
}
