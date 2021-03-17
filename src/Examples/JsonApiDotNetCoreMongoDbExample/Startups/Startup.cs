using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCoreMongoDbExample.Startups
{
    public sealed class Startup : EmptyStartup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISystemClock, SystemClock>();

            services.AddSingleton(_ =>
            {
                var client = new MongoClient(_configuration.GetSection("DatabaseSettings:ConnectionString").Value);
                return client.GetDatabase(_configuration.GetSection("DatabaseSettings:Database").Value);
            });

            services.AddJsonApi(ConfigureJsonApiOptions, facade => facade.AddCurrentAssembly());
            services.AddJsonApiMongoDb();

            services.AddScoped(typeof(IResourceReadRepository<>), typeof(MongoDbRepository<>));
            services.AddScoped(typeof(IResourceReadRepository<,>), typeof(MongoDbRepository<,>));
            services.AddScoped(typeof(IResourceWriteRepository<>), typeof(MongoDbRepository<>));
            services.AddScoped(typeof(IResourceWriteRepository<,>), typeof(MongoDbRepository<,>));
            services.AddScoped(typeof(IResourceRepository<>), typeof(MongoDbRepository<>));
            services.AddScoped(typeof(IResourceRepository<,>), typeof(MongoDbRepository<,>));
        }

        private void ConfigureJsonApiOptions(JsonApiOptions options)
        {
            options.IncludeExceptionStackTraceInErrors = true;
            options.Namespace = "api/v1";
            options.DefaultPageSize = new PageSize(5);
            options.IncludeTotalResourceCount = true;
            options.ValidateModelState = true;
            options.SerializerSettings.Formatting = Formatting.Indented;
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public override void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            app.UseRouting();
            app.UseJsonApi();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
