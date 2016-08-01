using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiotWadExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 1)
                throw new ArgumentException(nameof(args));
            string infile = args[0];
            if (!File.Exists(infile))
                throw new FileNotFoundException(infile);

            string outdirectory = Path.Combine(Path.GetFileNameWithoutExtension(infile));
            Directory.CreateDirectory(outdirectory);
                var wadfile = new WadFile(infile);
                Console.WriteLine($"Wad (Version: {wadfile.MajorVersion}.{wadfile.MinorVersion}) contains {wadfile.Entries.Length} files.");
                foreach (var entry in wadfile.Entries)
                {
                    string filename = HashToString(entry.FilenameHash) + GetExtensionByHeader(entry.UncompressedData);
                    Console.WriteLine($"Write file {filename}; Size: {entry.FileSize};\tUncompressed size: {entry.FileSizeUncompressed}");
                    File.WriteAllBytes(Path.Combine(outdirectory, filename), entry.UncompressedData);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.GetType()+";");
                Console.WriteLine("\t"+e.Message);
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        static StringBuilder sb = new StringBuilder();
        static string HashToString(byte[] hash)
        {
            sb.Length = 0;
            foreach (var b in hash)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
        static string GetExtensionByHeader(byte[] b)
        {
            if (CompareSeq(b,new byte[] {0xff,0xd8 })) return ".jpg";
            if (CompareSeq(b, Encoding.ASCII.GetBytes("OggS"))) return ".ogg";
            if (CompareSeq(b, Encoding.ASCII.GetBytes("PNG"), 1)) return ".png";
            if (CompareSeq(b, Encoding.ASCII.GetBytes("OTTO"))) return ".otf";
            if (CompareSeq(b, new byte[] { 0x1a, 0x45, 0xdf, 0xa3 })) return ".webm";
            if (CompareSeq(b, new byte[] { 0x00, 0x01, 0x00, 0x00 })) return ".ttf";

            return ".txt";
        }
        static bool CompareSeq(byte[] data, byte[] b,int offset=0)
        {
            if (b.Length+offset > data.Length) return false;
            for(int i=0;i<b.Length; i++)
                if (data[i+offset] != b[i]) return false;
            return true;
        }
    }
}
