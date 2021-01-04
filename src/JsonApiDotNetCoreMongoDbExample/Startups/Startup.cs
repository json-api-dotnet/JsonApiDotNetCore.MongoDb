using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb;
using JsonApiDotNetCoreMongoDbExample.Models;
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
            // TryAddSingleton will only register the IMongoDatabase if there is no
            // previously registered instance - will make tests use individual dbs
            services.TryAddSingleton(sp =>
            {
                var client = new MongoClient(Configuration.GetSection("DatabaseSettings:ConnectionString").Value);
                return client.GetDatabase(Configuration.GetSection("DatabaseSettings:Database").Value);
            });
            
            services.AddJsonApi(
                ConfigureJsonApiOptions,
                resources: builder =>
                {
                    builder.Add<Article, string>();
                    builder.Add<Author, string>();
                    builder.Add<Blog, string>();
                    builder.Add<Person, string>();
                    builder.Add<TodoItem, string>();
                    builder.Add<User, string>();
                });
            
            services.AddResourceRepository<MongoDbRepository<Article>>();
            services.AddResourceRepository<MongoDbRepository<Author>>();
            services.AddResourceRepository<MongoDbRepository<Blog>>();
            services.AddResourceRepository<MongoDbRepository<Person>>();
            services.AddResourceRepository<MongoDbRepository<TodoItem>>();
            services.AddResourceRepository<MongoDbRepository<User>>();

            // once all tests have been moved to WebApplicationFactory format we can get rid of this line below
            services.AddClientSerialization();
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
