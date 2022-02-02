using Backend.Models;
using Backend;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Backend.Context;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Backend.Models.Assets;

var localhostCors = "localhost";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "localhost",
        policy =>
        {
            policy.AllowAnyOrigin();
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            //policy.WithOrigins("https://localhost");
        });
});

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
} );

if(bool.Parse(builder.Configuration["UseInMemory"]))
{
    builder.Services.AddDbContext<ApplicationContext>(opt => opt.UseInMemoryDatabase("ApplicationDb"));
}
else
{
    builder.Services.AddDbContext<ApplicationContext>(opt => opt.UseSqlServer(builder.Configuration["ApplicationDb"]));
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swagger => 
{
    //This is to generate the Default UI of Swagger Documentation  
    swagger.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Draconity API",
        Description = "ASP.NET Core 6.0 Web API"
    });

    //... and tell Swagger to use those XML comments.
    swagger.IncludeXmlComments("../docs.xml");

    // To Enable authorization using Swagger (JWT)  
    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
    });

    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});

builder.Services.AddSingleton<IAuthorizationHandler, RequireScopeHandler>();

builder.Services.AddAuthorization(option =>
{
    foreach (PermissionType scope in Enum.GetValues(typeof(PermissionType)))
    {
        option.AddPolicy(scope.ToString(), policy =>
        {
            policy.AddRequirements( new[] { new ScopeRequirement(builder.Configuration["Jwt:Issuer"], scope) });
            policy.Build();
        });
    }

});

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
/*
.AddMicrosoftAccount(microsoftOptions =>
{
    microsoftOptions.ClientId = "";
    microsoftOptions.ClientSecret = "";
});
*/


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    BackendContext.Seed<ApplicationContext>(scope);
}

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseCors(localhostCors);
}

app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.UseAuthentication();

app.Run();
