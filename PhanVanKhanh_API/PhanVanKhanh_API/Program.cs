using Microsoft.EntityFrameworkCore;
using PhanVanKhanh_API.Data; // Namespace đúng
using PhanVanKhanh_API.Models; // Namespace đúng
using System.Text.Json.Serialization; // Thêm using này

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký DbContext với chuỗi kết nối "KhanhConn"
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("KhanhConn")));

// 2. Đăng ký các Repository và Interface
builder.Services.AddScoped<INhomRepository, NhomRepository>();
builder.Services.AddScoped<IThietBiRepository, ThietBiRepository>();

// 3. Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. Các dịch vụ mặc định
builder.Services.AddControllers()
        .AddJsonOptions(options => {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.WriteIndented = true;
        });

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