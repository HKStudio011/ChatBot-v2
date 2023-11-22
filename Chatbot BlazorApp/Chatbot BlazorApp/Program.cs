using Chatbot_BlazorApp.Client.Pages;
using Chatbot_BlazorApp.Components;
using Chatbot_BlazorApp.Models;
using Chatbot_BlazorApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services chatbot
builder.Services.AddSingleton<ChatbotModel>( provider =>
{
    string PathModel = builder.Configuration.GetSection("Model.PathModel").ToString();
    int NumberThread = int.Parse(builder.Configuration.GetSection("Model.NumberThread").ToString());
    string Version   = builder.Configuration.GetSection("Model.Version").ToString();
    var model = new ChatbotModel(PathModel,NumberThread,Version);
    model.Init().Wait();
    return model;
});

// Add DBContent
builder.Services.AddDbContext<ChatbotContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ChatbotContext"));
});


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

app.Run();
