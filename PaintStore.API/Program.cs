using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using PaintStore.API.Database;
using PaintStore.API.Repositories;
using PaintStore.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddDbContext<PaintStoreDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("PaintDb")));

builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<UsersRepository>();

builder.Services.AddScoped<PaintProductsService>();
builder.Services.AddScoped<PaintProductsRepository>();

builder.Services.AddScoped<OrdersService>();
builder.Services.AddScoped<OrdersRepository>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.MapControllers();

app.Run();
