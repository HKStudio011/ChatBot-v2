//using Chatbot_BlazorApp.Client.Pages;
using Chatbot_BlazorApp.Components;
using Chatbot_BlazorApp_Share.Services;
using Chatbot_BlazorApp_Share.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Chatbot_BlazorApp.Client.Pages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Chatbot API",
        Description = "An ASP.NET Core Web API for managing Chatbot services",
        //TermsOfService = new Uri("https://example.com/terms"),
        //Contact = new OpenApiContact
        //{
        //    Name = "Example Contact",
        //    Url = new Uri("https://example.com/contact")
        //},
        //License = new OpenApiLicense
        //{
        //    Name = "Example License",
        //    Url = new Uri("https://example.com/license")
        //}
    });
});

// Add DBContent
builder.Services.AddDbContext<ChatbotContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ChatbotContext"), b => b.MigrationsAssembly("Chatbot_BlazorApp"));
});

// Add HandleContent
builder.Services.AddTransient<IHandleContentService,HandleContentService>();

// Add HandleChat
builder.Services.AddTransient<IHandleChatService, HandleChatService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add services chatbot
builder.Services.AddSingleton<ChatbotModel>(provider =>
{
    string PathModel = builder.Configuration.GetSection("Model:PathModel").Value;
    int NumberThread = int.Parse(builder.Configuration.GetSection("Model:NumberThread").Value);
    string Version = builder.Configuration.GetSection("Model:Version").Value;
    var model = new ChatbotModel(PathModel, NumberThread, Version);
    model.Init().Wait();
    return model;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
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
    .AddAdditionalAssemblies(typeof(Chat).Assembly);


app.MapControllers();

app.Run();
