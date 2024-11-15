


using Ensek.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
void ConfigureServices(IServiceCollection services)
{
    var exepath = Path.GetDirectoryName(System.Reflection.Assembly
                                        .GetExecutingAssembly().Location);

    var configuration = new ConfigurationBuilder()
                            .SetBasePath(exepath)
                            .AddJsonFile("appsettings.json").Build();

    services.AddDbContext<MeterRecordContext>(options =>
    {
        options.UseSqlServer(configuration.GetSection("Ensek").Value);
    });


}

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

app.UseAuthorization();

app.MapControllers();

app.Run();
