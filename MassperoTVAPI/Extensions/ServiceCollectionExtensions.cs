using System.Reflection;
using System.Text;
using MassperoTV.Infrastructure.Repositories;
using MassperoTVAPI.Core.Entities;
using MassperoTVAPI.Core.Interfaces;
using MassperoTVAPI.Core.Interfaces.Repositories;
using MassperoTVAPI.Core.Settings;
using MassperoTVAPI.Infrastructure;
using MassperoTVAPI.Infrastructure.Data;
using MassperoTVAPI.Infrastructure.Repositories;
using MassperoTVAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace MassperoTVAPI.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers repositories, Unit of Work, and application services.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        // Generic repository — open generic registration
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Lookup repositories (one open-generic implementation, different type params)
        services.AddScoped<ILookupRepository<Status>,                  LookupRepository<Status>>();
        services.AddScoped<ILookupRepository<SecurityClearanceStatus>, LookupRepository<SecurityClearanceStatus>>();
        services.AddScoped<ILookupRepository<InterviewType>,           LookupRepository<InterviewType>>();
        services.AddScoped<ILookupRepository<Phase>,                   LookupRepository<Phase>>();
        services.AddScoped<ILookupRepository<ProcessStatus>,           LookupRepository<ProcessStatus>>();
        services.AddScoped<ILookupRepository<Location>,                LookupRepository<Location>>();

        // Domain entity repositories
        services.AddScoped<ICandidateRepository,       CandidateRepository>();
        services.AddScoped<IInterviewRepository,       InterviewRepository>();
        services.AddScoped<IJobRepository,             JobRepository>();
        services.AddScoped<ICategoryRepository,        CategoryRepository>();
        services.AddScoped<IIssueRepository,           IssueRepository>();
        services.AddScoped<IProcessRepository,         ProcessRepository>();
        services.AddScoped<IOfferRepository,           OfferRepository>();
        services.AddScoped<IConfigurationRepository,   ConfigurationRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Application services
        services.AddScoped<ITokenService,        TokenService>();
        services.AddScoped<IFileUploadService,   FileUploadService>();
        services.AddScoped<IConfigService,       ConfigService>();
        services.AddScoped<IExcelImportService,  ExcelImportService>();
        services.AddScoped<ICvParserService,      CvParserService>();

        // Email service
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    /// <summary>Configures ASP.NET Identity, JWT authentication, and authorization policies.</summary>
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services, IConfiguration config)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["JwtSettings:Issuer"],
                    ValidAudience = config["JwtSettings:Audience"],
                    IssuerSigningKey = key
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly",      policy => policy.RequireRole("Admin"));
            options.AddPolicy("HROnly",         policy => policy.RequireRole("HR"));
            options.AddPolicy("ClientOnly",     policy => policy.RequireRole("Client"));
            options.AddPolicy("CompanyOnly",    policy => policy.RequireRole("Company"));
            options.AddPolicy("AdminOrHR",      policy => policy.RequireRole("Admin", "HR"));
            options.AddPolicy("AdminOrCompany", policy => policy.RequireRole("Admin", "Company"));
        });

        return services;
    }

    /// <summary>Configures Swagger/OpenAPI with JWT Bearer security definition.</summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TV API",
                Version = "v1",
                Description = "Complaint & Inquiry Tracking System REST API",
                Contact = new OpenApiContact
                {
                    Name = "TV Support",
                    Email = "minashehata495@gmail.com"
                }
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter: **Bearer {your JWT token}**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}
