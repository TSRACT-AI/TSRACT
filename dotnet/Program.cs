using MudBlazor;
using MudBlazor.Services;
using TSRACT.Models;
using TSRACT.Repositories;
using TSRACT.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddCommandLine(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
});

builder.Services.AddSingleton<IdBaseRepository<LanguageModel>>();
builder.Services.AddSingleton<PromptLogRepository>();

builder.Services.AddSingleton<CloudStorageService>();
builder.Services.AddSingleton<LanguageModelService>();
builder.Services.AddSingleton<ModelTrainingService>();
builder.Services.AddSingleton<NodeProfileService>();
builder.Services.AddSingleton<OpenAiService>();
builder.Services.AddSingleton<PromptLogService>();

bool listenAllBindings = builder.Configuration.GetValue<bool?>("listen-all-bindings") ?? false;
bool bypassConda = builder.Configuration.GetValue<bool?>("bypass-conda") ?? true;

if (listenAllBindings)
{
    builder.WebHost.UseUrls("http://*:5000");
}
else
{
    builder.WebHost.UseUrls("http://localhost:5000");
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Start hardware monitoring
var nodeProfileService = app.Services.GetRequiredService<NodeProfileService>();
nodeProfileService.BypassConda = bypassConda;
nodeProfileService.StartMonitoring();

string pyPath = Path.Combine("..", "python");

// check if pyPath exists
if (!Directory.Exists(pyPath))
{
    Console.WriteLine($"Python path {pyPath} does not exist. Exiting.");
    Environment.Exit(1);
}
else
{
    Console.WriteLine($"Python path {pyPath} exists.");
}

if (!bypassConda)
{
    string envPath = Path.Combine("..", "python", "env");
    string condaBatPath = Path.Combine("..", "python", "conda", "condabin", "conda.bat");
    if (!Directory.Exists(envPath)) throw new Exception($"Python virtual environment path {envPath} does not exist. Exiting.");
    if (!Directory.Exists(condaBatPath)) throw new Exception($"Python conda.bat {condaBatPath} does not exist. Exiting.");
}

var languageModelService = app.Services.GetRequiredService<LanguageModelService>();
await languageModelService.ScanHfCache();

app.Run();
