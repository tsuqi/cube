﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cube
{ 
    public class Program
    {
        public static MPEGTCPServer MPEGServer = new MPEGTCPServer();

        public static async Task Main(string[] args)
        {
            await Task.Run(() => MPEGServer.Run()).ConfigureAwait(false);
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
