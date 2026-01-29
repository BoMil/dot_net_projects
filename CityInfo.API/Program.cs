using System.Reflection;
using Asp.Versioning;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CityInfo.API.DbContexts;
using CityInfo.API.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// Setup Serilog logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    // .WriteTo.File("logs/cityInfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

var envirnoment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
// builder.Logging.ClearProviders();
// builder.Logging.AddConsole();

if (envirnoment == Environments.Development)
{
    builder.Host.UseSerilog(
    (context, configuration) => configuration
        .MinimumLevel.Debug()
        .WriteTo.Console()
    );
}
else
{
    //! This is the example how to use Azure Key Vault to store the secrets
    // In case we use this code, we need to remove the azure key vault secrets from the appsettings.json file (SecretForKey for example)
    // var secretClient = new SecretClient(
    //     new Uri(builder.Configuration["KeyVault:Uri"]),  // Here goes the URI to your Key Vault that you can find in the Azure Portal
    //     new DefaultAzureCredential()
    // );

    // builder.Configuration.AddAzureKeyVault(
    //     secretClient,
    //     new KeyVaultSecretManager()
    // );

    builder.Host.UseSerilog(
     (context, configuration) => configuration
     .MinimumLevel.Debug()
     .WriteTo.Console()
     .WriteTo.File("logs/cityInfo.txt", rollingInterval: RollingInterval.Day)
     .WriteTo.ApplicationInsights(new TelemetryConfiguration()
     {
         // ! This key is located in the Azure Portal
         ConnectionString = builder.Configuration["ApplicationInsightsInstrumentationKey"]
     }, TelemetryConverter.Traces)
     .CreateLogger()
 );
}


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(options =>
{
    // This will return 406 if client requests an unsupported media type like xml for example.
    options.ReturnHttpNotAcceptable = true;
})
.AddNewtonsoftJson()
// Added this line to support xml requests.
.AddXmlDataContractSerializerFormatters();

// This will display proper error messages to the client without disposing the exception details
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        // This is used to add the xml comments to the swagger
        var xmlCommnetsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlCommnetsFile);
        options.IncludeXmlComments(xmlFilePath);

        // This will add Authorize btn in swagger, where we can enter token manually
        options.AddSecurityDefinition("CityInfoApiBearerAuth", new()
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            // Name = "Authorization",
            // In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "CityInfoApiBearerAuth",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                new List<string>()
            }
        });

    }
);
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

// This way we handle our dependency injection, it can be AddTransient, AddScoped, AddSingleton, etc.
#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();// In debug we use a local mail service
#else
builder.Services.AddTransient<IMailService, ClouldMailService>(); // In production we use a cloud mail service
#endif

builder.Services.AddDbContext<CityInfoContext>(
    dbContextOptions => dbContextOptions.UseSqlServer(builder.Configuration["ConnectionStrings:CityInfoDBConnectionString"])
);

builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// This middleware will require a JWT token to be passed in the Authorization header
builder.Services.AddAuthentication("Bearer").AddJwtBearer(
    options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Authentication:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["Authentication:SecretForKey"] ?? ""))
        };
    }
);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeFromAntwerp", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("city", "Antwerp");
    });
});

// Setup Versioning
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    // options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
}).AddMvc();

// Ovaj kod omogućava ASP.NET Core aplikaciji da ispravno prepozna pravi IP korisnika i 
// pravi protokol (http/https) kada radi iza proxy-ja (azure, nginx, etc.)
// Ovim govoriš ASP.NET Core aplikaciji da veruje HTTP headerima koje dodaje proxy / load balancer ispred tvoje aplikacije
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // ! X-Forwarded-Proto Kaže aplikaciji da li je originalni request bio http ili https
    // ! X-Forwarded-For Pravi IP adresu krajnjeg korisnika
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // This is used to configure the way we handle the exeptions in production
    // so that we can log them and return a proper response to the client.
    app.UseExceptionHandler();
}

app.UseForwardedHeaders();

// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CityInfo API v1");
    options.RoutePrefix = "swagger";
});
// }

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
