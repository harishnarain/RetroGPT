using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Ports;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<RelayService>();
builder.Services.AddHostedService<SerialPortListener>();

var app = builder.Build();

app.MapPost("/retro", async (HttpContext context, RelayService relayService) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var prompt = await reader.ReadToEndAsync();
    var response = await relayService.SendPromptToOpenAI(prompt);
    context.Response.ContentType = "text/plain";
    await context.Response.WriteAsync(response);
});

app.Run();