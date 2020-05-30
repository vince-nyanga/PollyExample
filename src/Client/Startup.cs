using System;
using System.Net.Http;
using System.Threading.Tasks;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddHttpClient<IWeatherService, HttpWeatherService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001/");
            })
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
        }

        private IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .AdvancedCircuitBreakerAsync(
                     failureThreshold: 0.5,
                     samplingDuration: TimeSpan.FromSeconds(5),
                     minimumThroughput: 2,
                     durationOfBreak: TimeSpan.FromSeconds(30), onBreak: OnBreak, onHalfOpen: OnHalfOpen, onReset: OnReset);
        }

        private void OnReset()
        {
            Log.Information("Circuit breaker onReset");
        }

        private void OnHalfOpen()
        {
            Log.Information("Circuit breaker onHalfOpen");
        }

        private void OnBreak(DelegateResult<HttpResponseMessage> arg1, TimeSpan arg2)
        {
            Log.Information("Circuit breaker onBreak");
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

      
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
