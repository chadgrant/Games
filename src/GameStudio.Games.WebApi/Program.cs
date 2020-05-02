using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using GameStudio.Secrets;

namespace GameStudio.Games.WebApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
#if AWS
			var secretsProvider = new AwsSecretsProvider(new GameStudio.Cloud.Aws.AwsAuthentication(),"dev_");
#endif
#if AZURE
			var secretsProvider = new AzureSecretsProvider("vaulturlfromsomewhere");
#endif
			return WebHost
				.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((context, config) =>
				{
					config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
						.AddSecrets(secretsProvider, 
							new Secret("mongo","connectionString"),
							new Secret("mongohealthcheck","conectionString")
						)
						.AddEnvironmentVariables("GAMES_");
				})
				.UseKestrel(k => k.AddServerHeader = false)
				.UseStartup<Startup>();
		}
	}
}