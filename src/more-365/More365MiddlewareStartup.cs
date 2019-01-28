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
            var more365Config = _configuration.GetSection("more-365");
            if(more365Config.Value == null)
            {
                more365Config = _configuration.GetSection("more365");
            }
            services.AddMore365Middleware(more365Config);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMore365Middleware();
        }
    }
}