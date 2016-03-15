using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiotWadExtractor
{
    class WadFile
    {
        public string Header { get; set; }
        public byte[] UnknownHeader { get; set; }
        public WadFileEntry[] Entries { get; set; }

        public WadFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename);

            using (FileStream fs = File.OpenRead(filename))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    //Read 2 bytes, usually ASCII "RW"
                    Header = "" + (char)br.ReadByte() + (char)br.ReadByte();
                    //Read 6 bytes, probably version number
                    byte[] h = new byte[6];
                    br.Read(h, 0, h.Length);
                    UnknownHeader = h;
                    //Read 4 bytes, uint32 number fo files
                    Entries = new WadFileEntry[br.ReadUInt32()];
                    for(int i=0;i<Entries.Length;i++)
                    {
                        //Read 24 bytes per entry
                        var entry = new WadFileEntry();
                        byte[] hash = new byte[8];
                        br.Read(hash, 0, hash.Length);
                        entry.Hash = hash;
                        entry.FileOffset = br.ReadUInt32();
                        entry.FileSize = br.ReadUInt32();
                        entry.FileSizeUncompressed = br.ReadUInt32();
                        entry.CompressionType= (CompressionType)br.ReadUInt32();
                        Entries[i] = entry;
                    }

                    while(fs.Position!=fs.Length)
                    {
                        long pos = fs.Position;
                        var entry=Entries.First(elem => elem.FileOffset == pos);
                        byte[] data=null;
                        switch(entry.CompressionType)
                        {
                            case CompressionType.None:
                                data = new byte[entry.FileSize];
                                br.Read(data, 0, data.Length);
                                break;
                            case CompressionType.Gzip:
                                data = new byte[entry.FileSize];
                                br.Read(data, 0, data.Length);
                                data = GzipDecompress(data, entry.FileSizeUncompressed);
                                break;

                        }
                        entry.UncompressedData = data;
                    }
                }
            }
        }
        private byte[] GzipDecompress(byte[] data, uint decompressedLength)
        {
            using (MemoryStream memStream = new MemoryStream(data))
            using (MemoryStream outStream = new MemoryStream((int)decompressedLength))
            using (GZipStream decStream = new GZipStream(memStream, CompressionMode.Decompress))
            {
                decStream.CopyTo(outStream);
                return outStream.ToArray();
            }
        }
    }
}
