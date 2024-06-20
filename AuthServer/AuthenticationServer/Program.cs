using AuthenticationServer.Core.AppSettings;
using AuthenticationServer.Core.Configs;
using AuthenticationServer.Core.Entities.IdentityAggregate;
using AuthenticationServer.Extensions;
using AuthenticationServer.Infrastructure.Data;
using AuthenticationServer.Infrastructure.Services;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var dbConnect = builder.Configuration.GetSection(DbConnectOptions.ConfigSectionPath).Get<DbConnectOptions>() ?? throw new Exception();

builder.Services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(dbConnect.ConnectionString));
builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

builder.Services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(dbConnect.ConnectionString);
                })
                .AddInMemoryIdentityResources(AuthenticationConfigs.GetIdentityResources())
                .AddInMemoryApiResources(AuthenticationConfigs.GetApiResources())
                .AddInMemoryApiScopes(AuthenticationConfigs.GetApiScopes())
                .AddInMemoryClients(AuthenticationConfigs.GetClients())
                .AddAspNetIdentity<AppUser>();

builder.Services.AddTransient<IProfileService, IdentityClaimsProfileService>();

builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
   .AllowAnyMethod()
   .AllowAnyHeader()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseExceptionHandler(builder =>
{
    builder.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            context.Response.AddApplicationError(error.Error.Message);
            await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
        }
    });
});

app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseIdentityServer();
app.MapDefaultControllerRoute();

app.Run();
