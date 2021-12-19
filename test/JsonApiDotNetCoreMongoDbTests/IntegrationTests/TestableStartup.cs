using JsonApiDotNetCore.Configuration;
using Microsoft.AspNetCore.Builder;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests;

public sealed class TestableStartup
{
    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseJsonApi();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
