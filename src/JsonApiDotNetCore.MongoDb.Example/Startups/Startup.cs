using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.MongoDb.Example.Services;
using JsonApiDotNetCore.QueryStrings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JsonApiDotNetCore.MongoDb.Example
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

            services.AddScoped<SkipCacheQueryStringParameterReader>();
            services.AddScoped<IQueryStringParameterReader>(sp => sp.GetRequiredService<SkipCacheQueryStringParameterReader>());

            // TryAddSingleton will only register the IMongoDatabase if there is no
            // previously registered instance - will make tests use individual dbs
            services.TryAddSingleton(sp =>
            {
                var client = new MongoClient(Configuration.GetSection("DatabaseSettings:ConnectionString").Value);
                return client.GetDatabase(Configuration.GetSection("DatabaseSettings:Database").Value);
            });

            services.AddResourceRepository<MongoEntityRepository<Address, string>>();
            services.AddResourceRepository<MongoEntityRepository<Article, string>>();
            services.AddResourceRepository<MongoEntityRepository<Author, string>>();
            services.AddResourceRepository<MongoEntityRepository<Blog, string>>();
            services.AddResourceRepository<MongoEntityRepository<Country, string>>();
            services.AddResourceRepository<MongoEntityRepository<IdentifiableArticleTag, string>>();
            services.AddResourceRepository<MongoEntityRepository<KebabCasedModel, string>>();
            services.AddResourceRepository<MongoEntityRepository<Passport, string>>();
            services.AddResourceRepository<MongoEntityRepository<Person, string>>();
            services.AddResourceRepository<MongoEntityRepository<Revision, string>>();
            services.AddResourceRepository<MongoEntityRepository<Models.Tag, string>>();
            services.AddResourceRepository<MongoEntityRepository<ThrowingResource, string>>();
            services.AddResourceRepository<MongoEntityRepository<TodoItem, string>>();
            services.AddResourceRepository<MongoEntityRepository<TodoItemCollection, string>>();
            services.AddResourceRepository<MongoEntityRepository<User, string>>();
            

            services.AddJsonApi(
                ConfigureJsonApiOptions,
                resources: builder =>
                {
                    builder.Add<Address, string>();
                    builder.Add<Article, string>();
                    builder.Add<Author, string>();
                    builder.Add<Blog, string>();
                    builder.Add<Country, string>();
                    builder.Add<IdentifiableArticleTag, string>();
                    builder.Add<KebabCasedModel, string>();
                    builder.Add<Passport, string>();
                    builder.Add<Person, string>();
                    builder.Add<Revision, string>();
                    builder.Add<Models.Tag, string>();
                    builder.Add<ThrowingResource, string>();
                    builder.Add<TodoItem, string>();
                    builder.Add<TodoItemCollection, string>();
                    builder.Add<User, string>();
                });

            // once all tests have been moved to WebApplicationFactory format we can get rid of this line below
            services.AddClientSerialization();
        }

        private void ConfigureClock(IServiceCollection services)
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
