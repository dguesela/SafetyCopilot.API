using Microsoft.EntityFrameworkCore;
using SafetyCopilot.API.Data;
using SafetyCopilot.API.Services;
using SafetyCopilot.API.Services.Interfaces;

var builder =
    WebApplication.CreateBuilder(args);


/* ============================================================
   Controllers
   ============================================================ */

builder.Services.AddControllers();


/* ============================================================
   Swagger
   ============================================================ */

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();


/* ============================================================
   Database
   ============================================================ */

builder.Services.AddDbContext<
    SafetyDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection")));


/* ============================================================
   Data Protection
   Used for encrypting OpenAI API keys
   ============================================================ */

builder.Services.AddDataProtection();


/* ============================================================
   Application Services
   ============================================================ */

builder.Services.AddScoped<
    ISecretEncryptionService,
    SecretEncryptionService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IProfileService,
    ProfileService>();

builder.Services.AddScoped<
    IProjectService,
    ProjectService>();

builder.Services.AddScoped<
    IPdfTextService,
    PdfTextService>();

builder.Services.AddScoped<
    IRequirementParserService,
    RequirementParserService>();

builder.Services.AddScoped<
    IRequirementDocumentService,
    RequirementDocumentService>();

builder.Services.AddScoped<
    IRequirementService,
    RequirementService>();

builder.Services.AddScoped<
    IAnalysisService,
    AnalysisService>();


/* ============================================================
   OpenAI HTTP Service
   ============================================================ */

builder.Services.AddHttpClient<
    IOpenAiClassificationService,
    OpenAiClassificationService>(
    client =>
    {
        client.BaseAddress =
            new Uri(
                "https://api.openai.com/");

        client.Timeout =
            TimeSpan.FromMinutes(5);
    });


/* ============================================================
   CORS
   ============================================================ */

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "ReactFrontend",
            policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:5173",
                        "http://localhost:5174")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });


var app =
    builder.Build();


/* ============================================================
   Middleware
   ============================================================ */

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors(
    "ReactFrontend");


/*
 * NO authentication middleware.
 * NO authorization middleware.
 *
 * This is intentional for the simplified
 * research prototype.
 */


app.MapControllers();


app.Run();