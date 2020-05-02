using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GameStudio.Repository.Document.Mongo;

namespace GameStudio.Games.Tests
{
	public static class TestConfig
	{
		static readonly Lazy<ServiceProvider> _serviceProvider = new Lazy<ServiceProvider>(() =>
		{
			var assemblies =
				Directory.GetFiles(AppContext.BaseDirectory, "GameStudio*.dll")
					.Select(p => AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(p, AppContext.BaseDirectory)))
					.ToArray();

			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
				.AddEnvironmentVariables("GAMES_");

			var root = builder.Build();

			var services = new ServiceCollection();
			services.AddAutoMapper(cfg => cfg.AddProfiles(assemblies));
			services.AddOptions();
			services.Configure<MongoOptions>(root.GetSection("mongo"));

			try
			{
				Mapper.Initialize(cfg => { cfg.AddProfiles(assemblies); });
			}
			catch (Exception ex)
			{
				if (!ex.Message.Contains("already initialized"))
					throw;
			}

			return services.BuildServiceProvider();
		});

		public static bool Initialize()
		{
			return _serviceProvider.Value != null;
		}

		public static ServiceProvider ServiceProvider => _serviceProvider.Value;
	}
}
