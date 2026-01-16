using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using pdf_generator_service.Exceptions;
using pdf_generator_service.Middlewares;
using pdf_generator_service.Services;
using pdf_generator_service.Services.Interface;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// Configure Serilog log settings from App Settings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// config json options
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}).ConfigureApiBehaviorOptions(options =>
// Customize model binding error's exception
{
    options.SuppressMapClientErrors = true;
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = context.ModelState
            .First(x => x.Value?.Errors.Count > 0).Value?.Errors.First().ErrorMessage;
        throw new BadRequestException(string.IsNullOrWhiteSpace(message) ? "" : message);
    };
}); ;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AMS API",
        Version = "v1",
        Description = "AMS API documentation"
    });

    // Read & load XML comments (enable XML generation in csproj first)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

// Add services to the container.
builder.Services.AddScoped<IPdfService, PdfService>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHsts();
app.UseHttpsRedirection();
app.UseMiddleware<StatusCodeMiddleware>();
app.UseRouting();
app.MapControllers();

app.Run();
