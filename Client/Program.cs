using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using Surveymatic.Shared;

namespace Surveymatic.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = baseAddress });

            builder.Services.AddHttpClient<SurveyHttpClient>(client => client.BaseAddress = baseAddress);


            builder.Services.AddSingleton<HubConnection>(sp => {
                var navigationManager = sp.GetRequiredService<NavigationManager>();
                return new HubConnectionBuilder()
                  .WithUrl(navigationManager.ToAbsoluteUri("/surveyhub"))
                  .WithAutomaticReconnect()
                  .Build();
            });

            var host = builder.Build();
            await host.RunAsync();

 
    }
}
}