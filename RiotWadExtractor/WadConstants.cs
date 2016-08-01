using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiotWadExtractor
{
    static class WadConstants
    {
        public static string WadMagic = "RW";
        public static byte[] SupportedVersions={1,2};
        public static byte[] V2UnknownHeader = {0x68, 0x00, 0x20, 0x00};
    }
}
