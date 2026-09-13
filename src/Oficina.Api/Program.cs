using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Context;
using System.Diagnostics;
using Oficina.Api.Middleware;
using Oficina.Application;
using Oficina.Infrastructure;
using Oficina.Infrastructure.Auth;
using Oficina.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Logging estruturado JSON (New Relic / observabilidade).
// Emite uma linha JSON por evento, com TraceId/SpanId para correlacao com o APM.
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service.name", "fiap-app")
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter()));

// swaagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Oficina API",
        Version = "v1",
        Description = "Sistema Integrado de Atendimento e Execução de Serviços — MVP Tech Challenge Fase 1 (SOAT/FIAP)."
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT — informe apenas o token (sem o prefixo \"Bearer \")."
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
    });
});

// DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Health checks (Fase 2): usados pelas probes do Kubernetes.
// /health/live  -> liveness  (o processo está de pé)
// /health/ready -> readiness (dependências prontas, ex.: banco)
var health = builder.Services.AddHealthChecks();

if (!builder.Environment.IsEnvironment("Testing"))
{
    var connStr = builder.Configuration.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException("ConnectionStrings:Postgres não configurada.");
    builder.Services.AddDbContext<OficinaDbContext>(o => o.UseNpgsql(connStr));
    health.AddDbContextCheck<OficinaDbContext>("postgres", tags: new[] { "ready" });
}

// fluent validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// JWT
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Seção Jwt ausente no appsettings.");

if (string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 32)
    throw new InvalidOperationException("Jwt:Secret deve ter ao menos 32 caracteres.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret))
        };
    });

builder.Services.AddAuthorization();

// cors
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});


var app = builder.Build();

// Aplica migrations EF Core no startup (para MVP n tem problema)
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
    if (ctx.Database.IsRelational())
        ctx.Database.Migrate();
}


// Correlacao: injeta trace.id/span.id (padrao New Relic) no contexto de log.
app.Use(async (context, next) =>
{
    var activity = Activity.Current;
    using (LogContext.PushProperty("trace.id", activity?.TraceId.ToString()))
    using (LogContext.PushProperty("span.id", activity?.SpanId.ToString()))
    {
        await next();
    }
});

// Log de requests HTTP (metodo, rota, status, latencia) em JSON.
app.UseSerilogRequestLogging();

// Tratamento global de erros (mapeia DomainException/AppException para HTTP)
app.UseMiddleware<ErrorHandlingMiddleware>();

// Swagger 
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Oficina API v1");
    c.RoutePrefix = string.Empty; // Swagger na raiz
});

app.UseCors();

// Autenticação e autorização (ordem importa)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoints de health para as probes do Kubernetes.
// Liveness: sempre responde se o processo está vivo (não checa dependências).
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
// Readiness: só fica pronto quando as dependências marcadas com a tag "ready" (ex.: Postgres) estão OK.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }
