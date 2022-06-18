using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexradDecoder
{
    static class Program
    {
        static void Main(string[] args)
        {
            var reflectivitydecoder = new NexradDecoder();
            reflectivitydecoder.Parse("E:\\Users\\Dennis\\Downloads\\KOKX_SDUS51_N0ROKX_202009292354");
            
            GraphicBlock graphic;
            if (reflectivitydecoder.DescriptionBlock.GraphicOffset != 0)
            {
                graphic = reflectivitydecoder.ParseGAB();
            }
        }
    } 
}
