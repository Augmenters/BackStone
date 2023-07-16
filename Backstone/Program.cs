using Geohash;
using Library.DataAccess;
using Library.DataAccess.Interfaces;
using Library.Repositories;
using Library.Repositories.Interfaces;
using Library.Repositories.Utilities;
using Library.Repositories.Utilities.Interfaces;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ConfigureLogging();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ISettings, Settings>();
builder.Services.AddScoped<Geohasher>();
builder.Services.AddScoped<IGridRepository, GridRepository>();
builder.Services.AddScoped<IYelpDataAccess, YelpDataAccess>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IAddressDataAccess, AddressDataAccess>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();

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

app.MapControllers();
app.Run();

