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
                        entry.CompressionType= (CompressionType)br.ReadUInt32();
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
                    }

                    while(fs.Position!=fs.Length)
                    {
                        long pos = fs.Position;
                        var entry=Entries.First(elem => elem.FileOffset == pos&&elem.FileSize!=0);
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
