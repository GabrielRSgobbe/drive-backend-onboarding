using Drive.Domain.Repositories;
using Drive.Domain.Storage;
using Drive.Infrastructure.Persistence;
using Drive.Infrastructure.Repositories;
using Drive.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Minio;

using Drive.Business.UseCases.UploadFile;
using Drive.Business.UseCases.ListFiles;
using Drive.Business.UseCases.DeleteFile;
using Drive.Business.UseCases.DownloadFile;

using Drive.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);

// ===============================
// Services
// ===============================

builder.Services.AddControllers();


// CORS (libera o front)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Front", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});



// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Drive.Api",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole: Bearer {seu_token}"
    });

    //  NOVO JEITO (OpenApi 2.x)
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});



// DB (Postgres)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Identity
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddSignInManager();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// MinIO client
var endpoint = builder.Configuration["Storage:Minio:Endpoint"]!;
var accessKey = builder.Configuration["Storage:Minio:AccessKey"]!;
var secretKey = builder.Configuration["Storage:Minio:SecretKey"]!;
var bucket = builder.Configuration["Storage:Minio:Bucket"] ?? "drive";
var useSSL = bool.Parse(builder.Configuration["Storage:Minio:UseSSL"] ?? "false");

builder.Services.AddSingleton<IMinioClient>(_ =>
{
    var c = new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey);

    if (useSSL) c = c.WithSSL();

    return c.Build();
});

// Domain interfaces -> Infrastructure implementations
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IStorageService>(sp =>
{
    var minio = sp.GetRequiredService<IMinioClient>();
    return new MinioStorageService(minio, bucket);
});

// Use Cases
builder.Services.AddScoped<UploadFileUseCase>();
builder.Services.AddScoped<ListFilesUseCase>();
builder.Services.AddScoped<DeleteFileUseCase>();
builder.Services.AddScoped<DownloadFileUseCase>();

var app = builder.Build();

// ===============================
// Pipeline HTTP
// ===============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors("Front");

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
