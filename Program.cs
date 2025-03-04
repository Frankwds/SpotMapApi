using Microsoft.EntityFrameworkCore;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";



// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
    });
});

// Add DbContext
builder.Services.AddDbContext<MarkerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MarkerContext")));


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(MyAllowSpecificOrigins);
app.UseHttpsRedirection();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<MarkerContext>();
}

app.MapGet("/markers", async (MarkerContext context, ILogger<Program> logger) =>
{
    logger.LogInformation("Markers endpoint was called.");
    return await context.Markers.ToListAsync();
}).WithName("GetMarkers").WithOpenApi();

app.MapPost("/markers", async (MarkerPost markerPost, MarkerContext context, ILogger<Program> logger) =>
{
    var newMarker = new Marker
    {
        Name = markerPost.Name,
        Position = markerPost.Position
    };
    context.Markers.Add(newMarker);
    await context.SaveChangesAsync();
    logger.LogInformation($"Marker was added: {newMarker}");
    return newMarker;
}).WithName("AddMarker").WithOpenApi();

app.MapDelete("/markers/{id}", async (int id, MarkerContext context, ILogger<Program> logger) =>
{
    var marker = await context.Markers.FindAsync(id);
    if (marker == null)
    {
        return Results.NotFound();
    }

    context.Markers.Remove(marker);
    await context.SaveChangesAsync();
    logger.LogInformation($"Marker was deleted. id: {id}");
    return Results.Ok(marker);
}).WithName("DeleteMarker").WithOpenApi();

app.Run();