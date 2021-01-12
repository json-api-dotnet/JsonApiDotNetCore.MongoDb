using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using JsonApiDotNetCore.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCoreMongoDbExample
{
    public class Startup : EmptyStartup
    {
        private IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration) : base(configuration)
        {
            Configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            ConfigureClock(services);
            
            // TryAddSingleton will only register the IMongoDatabase if there is no
            // previously registered instance - will make tests use individual dbs
            services.TryAddSingleton(sp =>
            {
                var client = new MongoClient(Configuration.GetSection("DatabaseSettings:ConnectionString").Value);
                return client.GetDatabase(Configuration.GetSection("DatabaseSettings:Database").Value);
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

        protected virtual void ConfigureClock(IServiceCollection services)
        {
            services.AddSingleton<ISystemClock, SystemClock>();
        }
        
        protected virtual void ConfigureJsonApiOptions(JsonApiOptions options)
        {
            options.IncludeExceptionStackTraceInErrors = true;
            options.Namespace = "api/v1";
            options.DefaultPageSize = new PageSize(5);
            options.IncludeTotalResourceCount = true;
            options.ValidateModelState = true;
            options.SerializerSettings.Formatting = Formatting.Indented;
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            app.UseRouting();
            app.UseJsonApi();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
