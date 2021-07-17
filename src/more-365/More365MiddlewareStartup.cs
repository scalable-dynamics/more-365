using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace more365
{
    public class More365MiddlewareStartup
    {
        private readonly IConfiguration _configuration;

        public More365MiddlewareStartup(IConfiguration config)
        {
            _configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMore365Middleware(_configuration);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMore365Middleware();
        }
    }
}