using Microsoft.EntityFrameworkCore;
using QLBH_API.Data;
using QLBH_API.Models;
using QLBH_API.Services;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký DbContext để kết nối CSDL
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("KhanhConn")));

// Cấu hình HttpClientFactory
builder.Services.AddHttpClient("githubApi", (serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>().GetSection("GitHubStorage");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", config["Token"]);
    client.DefaultRequestHeaders.Add("User-Agent", config["UserAgent"]);
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
});
builder.Services.AddHttpClient("githubRaw");

// Đăng ký Service
builder.Services.AddScoped<IGithubStorageService, GithubStorageService>();

// 2. Đăng ký các lớp Repository và Interface (Dependency Injection)
builder.Services.AddScoped<IDanhMucRepository, DanhMucRepository>();
builder.Services.AddScoped<ISanPhamRepository, SanPhamRepository>();
builder.Services.AddScoped<IGioHangRepository, GioHangRepository>();

// 3. Cấu hình CORS để cho phép Frontend gọi đến
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 5. Kích hoạt CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();