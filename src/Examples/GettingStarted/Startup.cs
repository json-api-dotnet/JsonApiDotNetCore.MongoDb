using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Configuration;
using JsonApiDotNetCore.MongoDb.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace GettingStarted
{
    public sealed class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_ =>
            {
                var client = new MongoClient(_configuration.GetSection("DatabaseSettings:ConnectionString").Value);
                return client.GetDatabase(_configuration.GetSection("DatabaseSettings:Database").Value);
            });

            services.AddJsonApi(ConfigureJsonApiOptions, resources: builder =>
            {
                builder.Add<Book, string>();
            });

            services.AddJsonApiMongoDb();

            services.AddResourceRepository<MongoRepository<Book, string>>();
        }

        private void ConfigureJsonApiOptions(JsonApiOptions options)
        {
            options.Namespace = "api";
            options.UseRelativeLinks = true;
            options.IncludeTotalResourceCount = true;
            options.SerializerSettings.Formatting = Formatting.Indented;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            CreateSampleData(app.ApplicationServices.GetService<IMongoDatabase>());

            app.UseRouting();
            app.UseJsonApi();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        private static void CreateSampleData(IMongoDatabase db)
        {
            db.GetCollection<Book>(nameof(Book)).InsertMany(new[]
            {
                new Book
                {
                    Title = "Frankenstein",
                    PublishYear = 1818,
                    Author = "Mary Shelley"
                },
                new Book
                {
                    Title = "Robinson Crusoe",
                    PublishYear = 1719,
                    Author = "Daniel Defoe"
                },
                new Book
                {
                    Title = "Gulliver's Travels",
                    PublishYear = 1726,
                    Author = "Jonathan Swift"
                }
            });
        }
    }
}
