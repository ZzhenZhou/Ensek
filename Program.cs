using Ensek;
using Ensek.Functions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

void ConfigureServices(IServiceCollection services)
{
    string? exepath = Path.GetDirectoryName(System.Reflection.Assembly
                                        .GetExecutingAssembly().Location);

    IConfigurationRoot configuration = new ConfigurationBuilder()
                            .SetBasePath(exepath)
                            .AddJsonFile("appsettings.json")
                            .Build();

    services.AddDbContext<EnsekDbContext>(options =>
    {
        string? connectionString = configuration.GetConnectionString("ENSEK");
        options.UseSqlServer(connectionString);

    });
    services.AddScoped<helperFunctions>();
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));


ConfigureServices(builder.Services);


WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

/*
 same entry twice, -> composite key lookup
checks for valid accountid, it is an integer and it does exist in the seeded file
checks the reading is of length of 5 letters and is all digits.

Improvments

non clustered index for the tables
create the tables using EF migrations. 
front end to ingest the file.
unit tests.
implement a dependency injection service.
 */