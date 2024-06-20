using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddIdentityServerAuthentication("IdentityServerBearer", options =>
                {
                    options.ApiName = "sampleapi";
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.Audience = "sampleapi";
                    options.RequireHttpsMetadata = false;
                });

builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("ApiReader", policy =>
                    {
                        policy.AuthenticationSchemes.Add("IdentityServerBearer");
                        policy.RequireClaim("scope", "api.read");
                    });

                    options.AddPolicy("SamplePolicy", policy =>
                    {
                        policy.AuthenticationSchemes.Add("IdentityServerBearer");
                        policy.RequireClaim("scope", "api.read");
                    });
                });


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Protected API", Version = "v1" });

    options.AddSecurityDefinition("IdentityServerBearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("http://localhost:5000/connect/authorize"),
                TokenUrl = new Uri("http://localhost:5000/connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    {"api.read", "Sample Api Scope"}
                }
            }
        }
    });

    options.AddSecurityDefinition("JwtBearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Please enter into field the word 'Bearer' followed by a space and the JWT value.",
    });

    options.OperationFilter<AuthorizeCheckOperationFilter>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample Api");
        options.OAuthClientId("sample_api_swagger");
        options.OAuthAppName("Sample Swagger Client");
        options.OAuthUsePkce();
    });
}

app.UseCors(options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>();

        if (authAttributes.Any())
        {
            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

            var securityRequirements = new List<OpenApiSecurityRequirement>();
            var identityServerScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "IdentityServerBearer" }
            };
            securityRequirements.Add(new OpenApiSecurityRequirement
            {
                [identityServerScheme] = new[] { "api.read" }
            });

            var jwtBearerScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "JwtBearer" }
            };
            securityRequirements.Add(new OpenApiSecurityRequirement
            {
                [jwtBearerScheme] = []
            });

            operation.Security = securityRequirements;
        }
    }
}