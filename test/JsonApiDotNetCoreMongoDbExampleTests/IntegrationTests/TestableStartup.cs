using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreMongoDbExample;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests
{
    public class TestableStartup : EmptyStartup
    {
        public TestableStartup(IConfiguration configuration) : base(configuration)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            app.UseRouting();
            app.UseJsonApi();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}