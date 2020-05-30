using System;
using System.Net.Http;
using Client.Interfaces;
using Client.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using Serilog;

namespace Client
{
    public class Startup
    {
       
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddHttpClient<IWeatherService, HttpWeatherService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001/");
            })
                .AddPolicyHandler(GetRetryPolicy());
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            Random jitterer = new Random();
            return HttpPolicyExtensions
                 .HandleTransientHttpError()
                 .OrResult(res => !res.IsSuccessStatusCode)
                 .WaitAndRetryAsync(
                      2,
                     retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                                           + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                     onRetry: (response, span, retryCount, context) =>
                     {
                         Log.Information("Retry count: {RetryCount}", retryCount);
                     });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
