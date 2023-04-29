using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;

namespace CustomerGrpc
{
	public class Program
	{
		public static void Main(string[] args)
		{
			try
			{
                CreateHostBuilder(args).Build().Run();
            }
			catch (Exception)
			{
				Console.WriteLine("Puerto no valido / conexion erronea");
			}
		}

		// Additional configuration is required to successfully run gRPC on macOS.
		// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.ConfigureKestrel(options =>
					{
						// Setup a HTTP/2 endpoint without TLS for OSX.
						options.ListenLocalhost(Int32.Parse(args[0]), o => o.Protocols = HttpProtocols.Http2);
					});
					webBuilder.UseStartup<Startup>();
				});
	}
}