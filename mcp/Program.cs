var builder = WebApplication.CreateBuilder(args);

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Add the MCP services: use HTTP transport instead of stdio.
builder.Services
    .AddMcpServer()
    .WithHttpTransport()          // ← Replaces .WithStdioServerTransport()
    .WithTools<OmgtuScheduleTools>()
    .WithTools<DateTimeTools>();

var app = builder.Build();

// Mount the MCP server to an HTTP endpoint (e.g., /mcp)
app.MapMcp("/mcp");               // Clients will connect to http://.../mcp

await app.RunAsync();
