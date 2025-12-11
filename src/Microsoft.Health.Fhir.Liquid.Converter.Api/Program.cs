using Microsoft.AspNetCore.Mvc;
using Microsoft.Health.Fhir.Liquid.Converter;
using Microsoft.Health.Fhir.Liquid.Converter.Api.Helper;
using Microsoft.Health.Fhir.Liquid.Converter.Api.Processor;
using Microsoft.Health.Fhir.Liquid.Converter.Api.Provider;
using Microsoft.Health.Fhir.Liquid.Converter.Helper;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();

// DI for converter components
builder.Services.AddSingleton<ITemplateProviderFactory, TemplateProviderFactory>();

builder.Services.AddSingleton<ProcessorSettings>(_ =>
    new ProcessorSettings()
    {
        EnableTelemetryLogger = false,
    });

builder.Services.AddSingleton<IDataParser, JsonDataParser>();
builder.Services.AddSingleton<Hl7v2Processor>();
builder.Services.AddSingleton<JsonProcessor>();
builder.Services.AddSingleton<CcdaProcessor>();

builder.Services.AddSingleton<IProcessorFactory, ProcessorFactory>();

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://0.0.0.0:80");
}

var app = builder.Build();

// Healthy
app.MapGet("/healthy", () =>
{
    return Results.Text("Healthy!");
});

// MetaData
app.MapGet("/metadata", ([FromServices] IHostEnvironment env) =>
{
    var baseDir = env.ContentRootPath;

    string metadataPath;

    if (env.IsDevelopment())
    {
        // Local debug (use parent directory logic)
        var root = Directory.GetParent(env.ContentRootPath)!.FullName;
        root = Directory.GetParent(root)!.FullName;

        metadataPath = Path.Combine(root, "data", "MetaData", "metadata.json");
    }
    else
    {
        // Production / staging / docker (use regular path)
        metadataPath = Path.Combine(env.ContentRootPath, "data", "MetaData", "metadata.json");
    }

    if (!File.Exists(metadataPath))
    {
        return Results.NotFound(new { error = "metadata.json not found at: " + metadataPath });
    }

    string json = File.ReadAllText(metadataPath);

    return Results.Text(json, "application/json");
});

// Hl7 Endpoint
app.MapPost("/convert/hl7", async (
    [FromServices] IProcessorFactory processorFactory,
    [FromServices] ITemplateProviderFactory templateFactory,
    HttpRequest req) =>
{
    return await EndpointHelper.ExecuteSafe(async () =>
    {
        using var reader = new StreamReader(req.Body);
        string input = await reader.ReadToEndAsync();

        var engine = processorFactory.GetProcessor(ProcessorTypes.Hl7v2);
        var provider = templateFactory.GetProvider(ProcessorTypes.Hl7v2);
        var messageType = Helper.GetHl7TemplateType(input);
        var sourceSystem = req.Headers["x-source-system"].FirstOrDefault() ?? "ADT";
        DateTime now = DateTime.UtcNow;
        string dynamicPath = $"Api/SourceSystems/{sourceSystem}/{now:yyyy}-{now:MM}-{now:dd}";
        string hl7Encrypted = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));

        // Assign it to the global variable
        TemplateGlobals.SourceSystemName = sourceSystem;
        TemplateGlobals.SourceSystemPath = dynamicPath;
        TemplateGlobals.HL7MessageCode = messageType;
        TemplateGlobals.Hl7Base64 = hl7Encrypted;

        var result = engine.Convert(input, messageType, provider);
        return Results.Text(result, "application/json");
    });
});

// json Endpoint
app.MapPost("/convert/json", async (
    [FromServices] IProcessorFactory processorFactory,
    [FromServices] ITemplateProviderFactory templateFactory,
    HttpRequest req) =>
{
    return await EndpointHelper.ExecuteSafe(async () =>
    {
        using var reader = new StreamReader(req.Body);
        string input = await reader.ReadToEndAsync();

        var engine = processorFactory.GetProcessor(ProcessorTypes.Json);
        var provider = templateFactory.GetProvider(ProcessorTypes.Json);

        var result = engine.Convert(input, "Stu3ChargeItem", provider);
        return Results.Text(result, "application/json");
    });
});

// ccda Endpoint
app.MapPost("/convert/ccda", async (
    [FromServices] IProcessorFactory processorFactory,
    [FromServices] ITemplateProviderFactory templateFactory,
    HttpRequest req) =>
{
    return await EndpointHelper.ExecuteSafe(async () =>
    {
        using var reader = new StreamReader(req.Body);
        string input = await reader.ReadToEndAsync();

        var engine = processorFactory.GetProcessor(ProcessorTypes.Ccda);
        var provider = templateFactory.GetProvider(ProcessorTypes.Ccda);

        var result = engine.Convert(input, "CCD", provider);
        return Results.Text(result, "application/json");
    });
});

// List templates
app.MapGet("/templates/list", ([FromServices] ITemplateProviderFactory templateFactory) =>
{
    var provider = templateFactory.GetProvider(ProcessorTypes.Hl7v2);
    var names = provider.ListAllTemplates();
    return Results.Ok(names);
});

app.Run();
