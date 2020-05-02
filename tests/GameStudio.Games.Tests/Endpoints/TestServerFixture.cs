using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using GameStudio.Games.WebApi;

namespace GameStudio.Games.Tests.Endpoints
{
	public class TestServerFixture : IDisposable
	{
		readonly TestServer _testServer;

		public TestServerFixture()
		{
			var builder = new WebHostBuilder()
				.UseContentRoot(GetContentRootPath())
				.UseEnvironment("Development")
				.ConfigureAppConfiguration((context, config) =>
				{
					config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
						.AddEnvironmentVariables("GAMES_");
				})
				.UseKestrel(k => k.AddServerHeader = false)
				.UseStartup<Startup>();

			_testServer = new TestServer(builder);
		}

		public HttpClient CreateClient()
		{
			return _testServer.CreateClient();
		}

		string GetContentRootPath()
		{
			var dir = new DirectoryInfo( Directory.GetCurrentDirectory() );
			while ((dir = dir.Parent) != null && dir.Name != "games" && dir.Name != "app") ;
			return Path.Combine(dir.FullName, "src","GameStudio.Games.WebApi");
		}

		public void Dispose()
		{
			_testServer.Dispose();
		}
	}
}
