using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GameStudio.Games.Repository.Mongo;
using GameStudio.References.Repository.Mongo;
using GameStudio.WebApi;


namespace GameStudio.Games.WebApi
{
    public class Startup : GameStudio.WebApi.Convention.StartupBase
	{
		public Startup(IConfiguration config) : base(config)
        {
        }

        readonly string MyAllowAllOrigins = "_myAllowAllOrigins";

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<GameMapper>();
            services.AddSingleton<ReferenceMapper>();

            // Setup CORS to allow requests from same domain, Angular port
            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowAllOrigins,
                builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            try
			{
				Mapper.Initialize(cfg => { cfg.AddProfiles(Assemblies); });
			}
			catch (Exception ex)
			{
				if (!ex.Message.Contains("already initialized"))
					throw;
			}

            base.ConfigureServices(services);
		}

        public override IEnumerable<Type> GetFilters()
        {
            return base.GetFilters().Concat(new []{typeof(ControllerMetricsFilter)});
        }


        protected override ApiInfo CreateInfoForApiVersion(ApiVersionDescription version)
		{
			return new ApiInfo
			{
				Title = $"Games API {version.ApiVersion}",
				Version = version.ApiVersion.ToString(),
				Description = "A sample application with Swagger, Swashbuckle, and API versioning.",
				Contact = new ApiContact { Name = "Chad Grant", Email = "cgrant@studio.com" }
			};
		}

        public override void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(MyAllowAllOrigins);
            base.Configure(app, env);
        }
    }
}