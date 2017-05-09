using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public byte MajorVersion { get; set; }
        public byte MinorVersion { get; set; }
        public byte[] UnknownHeader { get; set; }
        public byte[] UnknownHeader2 { get; set; }
        public WadFileEntry[] Entries { get; set; }

        public WadFile(string filename)
        {
            Dictionary<long, int> NumberOfAliases=new Dictionary<long, int>();
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename);

            using (FileStream fs = File.OpenRead(filename))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    //Read 2 bytes, ASCII "RW"
                    Header = "" + (char)br.ReadByte() + (char)br.ReadByte();

                    if(Header!=WadConstants.WadMagic)
                        throw new WadDeserializationException($"Unknown header value: {Header}");
                    MajorVersion = br.ReadByte();
                    MinorVersion = br.ReadByte();
                    if(!WadConstants.SupportedVersions.Any(elem=>elem==MajorVersion))
                        throw new WadDeserializationException($"Unsupported version: {MajorVersion}");
                    //Read 6 bytes, probably version number
                    byte[] h = null;
                    switch (MajorVersion)
                    {
                        case 1:
                        h = new byte[4];
                            break;
                        case 2:
                            h = new byte[92];
                            break;
                        default:
                            throw new WadDeserializationException($"Unsupported version: {MajorVersion}");
                    }
                    br.Read(h, 0, h.Length);
                    UnknownHeader = h;

                    if (MajorVersion == 2)
                    {
                        UnknownHeader2=new byte[4];
                        br.Read(UnknownHeader2, 0, UnknownHeader2.Length);
                        if (!new byte[] {0x68, 0x00, 0x20, 0x00}.SequenceEqual(UnknownHeader2))
                        {
                            throw new WadDeserializationException($"'Unknown' V2UnknownHeader: {string.Join(", ",UnknownHeader2)}");
                        }
                    }
                    //Read 4 bytes, uint32 number fo files
                    Entries = new WadFileEntry[br.ReadUInt32()];
                    for(int i=0;i<Entries.Length;i++)
                    {
                        var entry = new WadFileEntry();
                        byte[] hash = new byte[8];
                        br.Read(hash, 0, hash.Length);
                        entry.FilenameHash = hash;
                        entry.FileOffset = br.ReadUInt32();
                        entry.FileSize = br.ReadUInt32();
                        entry.FileSizeUncompressed = br.ReadUInt32();
                        entry.CompressionType= (CompressionType)(byte)br.ReadUInt32();
                        if(!Enum.IsDefined(typeof(CompressionType),entry.CompressionType))
                            throw new WadDeserializationException($"Unknown CompressionType: {entry.CompressionType}");
                        if (entry.FileSize == 0)
                            entry.UncompressedData = new byte[0];
                        if (MajorVersion == 2)
                        {
                            hash = new byte[8];
                            br.Read(hash, 0, hash.Length);
                            entry.UnknownHashOrChecksum = hash;
                        }
                        Entries[i] = entry;
                        if (NumberOfAliases.ContainsKey(entry.FileOffset))
                            NumberOfAliases[entry.FileOffset]++;
                        else
                            NumberOfAliases[entry.FileOffset]=1;
                    }

                    while(fs.Position!=fs.Length)
                    {
                        long pos = fs.Position;
                        byte[] data = null;
                        for (int i=0,j=0;i<Entries.Length&&j<NumberOfAliases[pos];i++)
                        {
                            var entry = Entries[i];
                            if (entry.FileOffset==pos)
                            {
                                if (pos==fs.Position)
                                {
                                    data = new byte[entry.FileSize];
                                    br.Read(data, 0, data.Length);
                                    switch (entry.CompressionType)
                                    {
                                        case CompressionType.None:
                                            break;
                                        case CompressionType.Gzip:
                                            data = GzipDecompress(data, entry.FileSizeUncompressed);
                                            break;
                                    }
                                }
                                entry.UncompressedData = data;
                                j++;
                            }
                        }
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
