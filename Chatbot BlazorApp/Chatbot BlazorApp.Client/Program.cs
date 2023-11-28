using Chatbot_BlazorApp_Share.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton(httpclient => new HttpClient()
{
    BaseAddress = new Uri(builder.Configuration.GetSection("API url").Value)
});

builder.Services.AddScoped<IHandleContentService,HandleContentClientService>();

builder.Services.AddScoped<IHandleChatService, HandleChatClientService>();

await builder.Build().RunAsync();
