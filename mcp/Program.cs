var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options => {options.EnableLegacySse = true;})
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp();

await app.RunAsync();
