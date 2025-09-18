using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            // temporário para desenvolvimento. Em produção restrinja origins.
            .AllowAnyOrigin()
    );
});

var app = builder.Build();

app.UseCors();
app.MapHub<ChatHub>("/hub");

// endpoint que o content-script usará para enviar eventos (alternativa ao SignalR invoke)
app.MapPost("/api/events", async (HttpContext ctx, IHubContext<ChatHub> hub) =>
{
    try
    {
        var ev = await JsonSerializer.DeserializeAsync<IncomingEvent>(ctx.Request.Body);
        if (ev != null)
        {
            // broadcast para frontend e para todos content-scripts
            await hub.Clients.All.SendAsync("NewMessage", ev);
            return Results.Ok(new { ok = true });
        }
        return Results.BadRequest();
    }
    catch (System.Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/", () => "WhatsApp Proxy API running");

app.Run();

// Models & Hub (small, inline for clarity)
// In a real project coloque em arquivos separados.

public class IncomingEvent
{
    public string ContactNumber { get; set; } = "";
    public string ContactName { get; set; } = "";
    public string Text { get; set; } = "";
    public long Timestamp { get; set; }
    public string? ChatId { get; set; }
}

public class ChatHub : Hub
{
    // Recebe invocações do content-script via SignalR
    public async Task FromContentScript(IncomingEvent ev)
    {
        // aqui a lógica: salvar, logar, encaminhar
        await Clients.All.SendAsync("NewMessage", ev);
    }

    // O frontend pode chamar este método para enviar comando ao content-script
    public async Task SendCommandToClient(string connectionId, object command)
    {
        await Clients.Client(connectionId).SendAsync("Command", command);
    }
}
