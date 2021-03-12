using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCoreMongoDbExample.Startups;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests
{
    public sealed class TestableStartup : EmptyStartup
    {
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
