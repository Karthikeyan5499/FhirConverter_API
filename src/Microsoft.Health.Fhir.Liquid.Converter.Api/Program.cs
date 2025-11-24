using Microsoft.AspNetCore.Mvc;
using Microsoft.Health.Fhir.Liquid.Converter;
using Microsoft.Health.Fhir.Liquid.Converter.Api.Helper;
using Microsoft.Health.Fhir.Liquid.Converter.Api.Processor;
using Microsoft.Health.Fhir.Liquid.Converter.Api.Provider;
using Microsoft.Health.Fhir.Liquid.Converter.Helper;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Parsers;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;

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

var app = builder.Build();

// MetaData
app.MapGet("/metadata", ([FromServices] IHostEnvironment env) =>
{
    var baseDir = env.ContentRootPath;

    var root = Directory.GetParent(env.ContentRootPath)!.FullName;
    root = Directory.GetParent(root)!.FullName;

    // Path to the metadata.json file
    var metadataPath = Path.Combine(root, "data", "MetaData", "metadata.json");

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
