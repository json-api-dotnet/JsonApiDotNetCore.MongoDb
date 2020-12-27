using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb;
using JsonApiDotNetCore.MongoDb.GettingStarted.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.MongoDb.GettingStarted
{
    public sealed class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var client = new MongoClient(Configuration.GetSection("DatabaseSettings:ConnectionString").Value);
                return client.GetDatabase(Configuration.GetSection("DatabaseSettings:Database").Value);
            });

            services.AddResourceRepository<MongoEntityRepository<Book, string>>();
            services.AddJsonApi(options =>
            {
                options.Namespace = "api";
                options.UseRelativeLinks = true;
                options.IncludeTotalResourceCount = true;
                options.SerializerSettings.Formatting = Formatting.Indented;
            }, resources: builder =>
            {
                builder.Add<Book, string>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseJsonApi();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
