using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TechGadgets.API.Data.Context;
using TechGadgets.API.Models;
using TechGadgets.API.Services.Implementations;
using TechGadgets.API.Services.Interfaces;
using System.Reflection;
using AspNetCoreRateLimit;
using TechGadgets.API.Services.Implementation;
using TechGadgets.API.Configuration;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using SixLabors.ImageSharp.Web.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// CONFIGURACIÓN DE SERVICIOS
// =====================================================

// Configurar JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// ✅ CONFIGURAR SUPABASE SETTINGS
builder.Services.Configure<SupabaseSettings>(
    builder.Configuration.GetSection(SupabaseSettings.SectionName));

// Configurar Entity Framework
builder.Services.AddDbContext<TechGadgetsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configurar Autenticación JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null)
{
    throw new InvalidOperationException("JWT Settings no configurado correctamente");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Solo para desarrollo
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true
    };

    // Configurar eventos para debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully");
            return Task.CompletedTask;
        }
    };
});

// Configurar Autorización
builder.Services.AddAuthorization(options =>
{
    // Políticas personalizadas
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Cliente"));
    options.AddPolicy("CanManageProducts", policy => policy.RequireClaim("permission", "productos.crear", "productos.editar"));
});

// Registrar HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Registrar servicios personalizados
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISlugService, SlugService>();
builder.Services.AddScoped<IProductosImagenService, ProductosImagenService>();
builder.Services.AddScoped<ISupabaseStorageService, SupabaseStorageService>();

// ✅ CONFIGURAR LÍMITES DE SUBIDA DE ARCHIVOS
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// ✅ CONFIGURAR KESTREL PARA MANEJAR ARCHIVOS GRANDES
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

// ✅ CONFIGURAR IIS PARA MANEJAR ARCHIVOS GRANDES (si usas IIS)
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

// ✅ INSTALAR SIXLABORS.IMAGESHARP PARA PROCESAMIENTO DE IMÁGENES
// Agregar esta línea si no está ya configurado
builder.Services.AddImageSharp();

// Configurar Controllers
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Personalizar respuesta de validación
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var response = new
            {
                success = false,
                message = "Errores de validación",
                errors = errors
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });

// Configurar Swagger/OpenAPI con autenticación
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TechGadgets API",
        Version = "v1.0.0",
        Description = "API para e-commerce de gadgets tecnológicos",
        Contact = new OpenApiContact
        {
            Name = "TechGadgets Support",
            Email = "support@techgadgets.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Configurar autenticación JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingresa el token JWT en el formato: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Configurar anotaciones de Swagger
    c.EnableAnnotations();
    
    // Configurar documentación XML si existe
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configurar Rate Limiting
builder.Services.Configure<AspNetCoreRateLimit.IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<AspNetCoreRateLimit.RateLimitRule>
    {
        new AspNetCoreRateLimit.RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 100
        }
    };
});

// Configurar memoria caché
builder.Services.AddMemoryCache();
builder.Services.Configure<AspNetCoreRateLimit.IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimit"));
builder.Services.Configure<AspNetCoreRateLimit.IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<AspNetCoreRateLimit.IRateLimitConfiguration, AspNetCoreRateLimit.RateLimitConfiguration>();

// =====================================================
// CONFIGURACIÓN DE LA APLICACIÓN
// =====================================================

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.SerializeAsV2 = false; // Forzar OpenAPI 3.0
    });
    
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechGadgets API v1.0.0");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
        c.DocumentTitle = "TechGadgets API Documentation";
        c.DefaultModelsExpandDepth(-1); // Ocultar modelos por defecto
        c.DisplayRequestDuration();
        c.EnableFilter();
        c.EnableDeepLinking();
        c.EnableValidator();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });

    // Usar páginas de error detalladas en desarrollo
    app.UseDeveloperExceptionPage();
}
else
{
    // Manejo de errores en producción
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Middleware de seguridad
app.UseHttpsRedirection();

// CORS debe ir antes de Authentication
app.UseCors("AllowSpecificOrigin");

// Rate limiting
app.UseIpRateLimiting();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Middleware para logging de requests (usando ILogger nativo)
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"{context.Request.Method} {context.Request.Path} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    await next();
});

// Configurar rutas de controllers
app.MapControllers();


// Endpoint para información de la API
app.MapGet("/", () => new 
{ 
    name = "TechGadgets API",
    version = "1.0.0",
    description = "API para e-commerce de gadgets tecnológicos",
    swagger = "/swagger",
    health = "/health"
})
.WithName("ApiInfo")
.WithTags("Info");

// =====================================================
// INICIALIZACIÓN DE DATOS (OPCIONAL)
// =====================================================

// Crear scope para inicializar datos si es necesario
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TechGadgetsDbContext>();
    
    try
    {
        // Verificar conexión a la base de datos
        await context.Database.CanConnectAsync();
        Console.WriteLine("✅ Conexión a base de datos exitosa");
        
        // Opcional: Crear datos iniciales si no existen
        // await SeedDataAsync(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error de conexión a base de datos: {ex.Message}");
        // En producción, podrías querer que la aplicación no inicie si no hay conexión a BD
        // throw;
    }
}

Console.WriteLine("🚀 TechGadgets API iniciada correctamente");
Console.WriteLine($"🌐 Swagger UI disponible en: {(app.Environment.IsDevelopment() ? "https://localhost:5260" : "")}");

app.Run();

// =====================================================
// MÉTODOS AUXILIARES
// =====================================================

/*
// Método para inicializar datos (opcional)
static async Task SeedDataAsync(TechGadgetsDbContext context)
{
    // Verificar si ya existen datos
    if (await context.Usuarios.AnyAsync())
        return;

    // Crear usuario administrador por defecto
    var adminUser = new Usuario
    {
        UsuEmail = "admin@techgadgets.com",
        UsuPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
        UsuNombre = "Administrador",
        UsuApellido = "Sistema",
        UsuActivo = true,
        UsuEmailVerificado = true,
        UsuFechaCreacion = DateTime.UtcNow,
        UsuFechaModificacion = DateTime.UtcNow
    };

    context.Usuarios.Add(adminUser);
    await context.SaveChangesAsync();

    Console.WriteLine("👤 Usuario administrador creado");
}
*/