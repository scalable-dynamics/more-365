using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using more365.Graph;
using System;

namespace more365
{
    public static class More365Middleware
    {
        public static IServiceCollection AddMore365(this IServiceCollection services, IConfiguration config)
        {
            return services.AddMore365(config.GetSection("more365").Get<More365Configuration>());
        }

        public static IServiceCollection AddMore365(this IServiceCollection services, More365Configuration config)
        {
            services
                .AddSingleton(config)
                .AddSingleton<IAuthenticatedHttpClientFactory, AuthenticatedHttpClientFactory>()
                .AddSingleton<IMore365ClientFactory, More365ClientFactory>()
                .AddScoped(s => s.GetRequiredService<IMore365ClientFactory>().CreateDynamicsClient())
                .AddScoped(s => s.GetRequiredService<IMore365ClientFactory>().CreateGraphClient())
                .AddScoped(s => s.GetRequiredService<IMore365ClientFactory>().CreateSharePointClient());
            return services;
        }

        public static IServiceCollection AddMore365Middleware(this IServiceCollection services, IConfiguration config)
        {
            var more365Config = config.GetSection("more365").Get<More365Configuration>();

            services.AddCors();

            services
                .AddScoped(s =>
                {
                    var authenticatedHttpClientFactory = s.GetRequiredService<IAuthenticatedHttpClientFactory>();
                    var httpClient = authenticatedHttpClientFactory.CreateAuthenticatedHttpClient(more365Config.DynamicsUrl);
                    httpClient.BaseAddress = more365Config.DynamicsUrl;
                    httpClient.Timeout = new TimeSpan(0, 2, 0);
                    return new HttpProxyService(httpClient);
                });

            services.AddHttpClient(GraphClient.MicrosoftGraphUrl.ToString());

            return services.AddMore365(more365Config);
        }

        public static void UseMore365Middleware(this IApplicationBuilder builder)
        {
            builder.UseCors(b =>
            {
                b.AllowAnyOrigin()
                 .AllowAnyHeader()
                 .AllowAnyMethod();
            });

            builder.MapWhen(c =>
            {
                return c.Request.Path.Value.StartsWith("/api/data/", StringComparison.InvariantCultureIgnoreCase)
                    || c.Request.Path.Value.StartsWith("/webresources/", StringComparison.InvariantCultureIgnoreCase);
            }, b =>
            {
                b.Run(async context =>
                {
                    var proxy = context.RequestServices.GetRequiredService<HttpProxyService>();
                    await proxy.ProxyRequest(context.Request, context.Response, context.RequestAborted);
                });
            });
        }
    }
}