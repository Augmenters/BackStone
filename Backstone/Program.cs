using Library.DataAccess;
using Library.Repositories;
using Library.Repositories.Interfaces;
using Library.Repositories.Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ISettings, Settings>();
builder.Services.AddScoped<IYelpDataAccess, YelpDataAccess>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

