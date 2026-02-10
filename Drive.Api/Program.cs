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

var builder = WebApplication.CreateBuilder(args);

// ===============================
// Services
// ===============================

// Controllers (vamos usar controllers, não só minimal API)
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB (Postgres)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

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

// (Auth virá aqui depois)
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

app.Run();
