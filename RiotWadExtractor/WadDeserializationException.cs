using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiotWadExtractor
{
    class WadDeserializationException:Exception
    {
        public WadDeserializationException(string message) : base(message)
        {
        }
    }
}
