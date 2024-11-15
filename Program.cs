using Ensek;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
void ConfigureServices(IServiceCollection services)
{
    var exepath = Path.GetDirectoryName(System.Reflection.Assembly
                                        .GetExecutingAssembly().Location);

    // Load configuration
    var configuration = new ConfigurationBuilder()
                            .SetBasePath(exepath)
                            .AddJsonFile("appsettings.json")
                            .Build();

    // Register the DbContext with the configuration
    services.AddDbContext<EnsekDbContext>(options =>
    {
        var connectionString = configuration.GetConnectionString("ENSEK");
        options.UseSqlServer(connectionString);

    });

    services.AddScoped<EnsekDbContext>();

    // Add controllers and other services
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}

ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
