using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FileUploadService.Configuration;
using FileUploadService.Repositories;
using FileUploadService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register DB default storage
var folder = Environment.SpecialFolder.LocalApplicationData;
string path = Environment.GetFolderPath(folder);
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite($"Data Source={Path.Join(path, "service.db")}"));

// register Jwt configuration
string? jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
string? jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();
if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtKey)) {
    throw new ApplicationException("Invalid JWT config");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options => {
     options.TokenValidationParameters = new TokenValidationParameters {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtIssuer,
         ValidAudience = jwtIssuer,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
     };
 });

// Register services
builder.Services.AddTransient<IFileRepository, FileRepository>();
builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<IStorageService, StorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
