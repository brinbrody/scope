using DGScope.Receivers.ReadsbJSON;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScopeServer
{
    public class Program
    {
        string[] pluginPaths;
        public static void Main(string[] args)
        {
            
            Settings.LoadFacilities();
            Settings.StartReceivers();
            Settings.AdsbReceivers.Add(
                new ReadsbJSONReceiver() 
                {
                    Host = "localhost",
                    Port = 30006,
                    Enabled = true
                }
            );
            var adsb = new AdsbInput(Settings.AdsbReceivers, Settings.Facilities);
            adsb.Start();
            CreateHostBuilder(args).Build().Run();

        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
