using DGScope.Library;
using DGScope.Receivers;
using System;
using System.Collections.Generic;

namespace DGScope.AdsbUploadClient
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var uploader = new AdsbUploader();
            uploader.Run();
        }
    }
}
