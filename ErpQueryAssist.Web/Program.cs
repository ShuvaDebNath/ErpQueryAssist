using ErpQueryAssist.Application.Interfaces;
using ErpQueryAssist.Application.Services;
using ErpQueryAssist.Application.Settings;
using ErpQueryAssist.Infrastructure.Persistence;
using ErpQueryAssist.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var openAISection = builder.Configuration.GetSection("OpenAI");

builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(defaultConnection));

builder.Services.Configure<OpenAIOptions>(openAISection);

builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<ILlmService, LlmService>();
builder.Services.AddSingleton<PromptTemplateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
