using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace sharpsfv
{
    public class SfvBuilder
    {
        private readonly Dictionary<string, uint> _entries = new Dictionary<string, uint>();

        public void AddFile(string path)
        {
            AddStream(Path.GetFileName(path), File.OpenRead(path));
        }

        public async Task AddFileAsync(string path)
        {
            await AddStreamAsync(Path.GetFileName(path), File.OpenRead(path));
        }

        public void AddStream(string key, Stream stream)
        {
            var crc = new Crc32();
            var useStream = stream as BufferedStream ?? new BufferedStream(stream);

            byte[] buffer = new byte[0x1000];
            int bytesRead;
            while((bytesRead = useStream.Read(buffer, 0, 0x1000)) > 0)
                crc.Update(buffer, 0, bytesRead);

            _entries.Add(key, crc.Value);
        }

        public async Task AddStreamAsync(string key, Stream stream)
        {
            var crc = new Crc32();
            var useStream = stream as BufferedStream ?? new BufferedStream(stream);

            byte[] buffer = new byte[0x1000];
            int bytesRead;
            while ((bytesRead = await useStream.ReadAsync(buffer, 0, 0x1000)) > 0)
                crc.Update(buffer, 0, bytesRead);

            _entries.Add(key, crc.Value);
        }

        public IReadOnlyDictionary<string, uint> Entries
        {
            get
            {
                return new ReadOnlyDictionary<string, uint>(_entries);
            }
        }
    }
}
