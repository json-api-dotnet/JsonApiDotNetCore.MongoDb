using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreMongoDbExample.Startups
{
    /// <summary>
    /// Empty startup class, required for integration tests.
    /// Changes in ASP.NET Core 3 no longer allow Startup class to be defined in test projects. See https://github.com/aspnet/AspNetCore/issues/15373.
    /// </summary>
    public abstract class EmptyStartup
    {
        public virtual void ConfigureServices(IServiceCollection services)
        {
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
        }
    }
}
