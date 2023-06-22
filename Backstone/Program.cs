using Geohash;
using Library.DataAccess;
using Library.Repositories;
using Library.Repositories.Interfaces;
using Library.Repositories.Utilities;
using Library.Repositories.Utilities.Interfaces;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ConfigureLogging();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ISettings, Settings>();
builder.Services.AddScoped<Geohasher>();
builder.Services.AddScoped<IGridRepository, GridRepository>();
builder.Services.AddScoped<IYelpDataAccess, YelpDataAccess>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();

var cacheSize = builder.Services.BuildServiceProvider().GetRequiredService<ISettings>().CacheSize;
builder.Services.AddMemoryCache(x => new MemoryCacheOptions().SizeLimit = cacheSize);
builder.Services.AddScoped<ICacheHelper, CacheHelper>();

var app = builder.Build();

app.UseSwagger();
app.UseStaticFiles();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "Backstone";
    options.HeadContent = File.ReadAllText("wwwroot/swagger-ui/sidebar.html");
    options.InjectStylesheet("/swagger-ui/SwaggerDark.css");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

