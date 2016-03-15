using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiotWadExtractor
{
    class WadFileEntry
    {
        public byte[] Hash { get; set; }
        public uint FileOffset { get; set; }
        public uint FileSize { get; set; }
        public uint FileSizeUncompressed { get; set; }
        public CompressionType CompressionType { get; set; }
        public byte[] UncompressedData { get; set; }
    }
}
