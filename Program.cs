using Microsoft.Extensions.FileProviders;

var specificOrigins = "_specificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy(name:specificOrigins,
        policy =>
        {
            policy.WithOrigins("https://localhost:7063");
            policy.WithOrigins("https://justpostit.azurewebsites.net");
        });
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(),@"wwwroot/images")),
    RequestPath = new PathString("/api/images")
});

app.UseDirectoryBrowser(new DirectoryBrowserOptions()
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(),@"wwwroot/images")),
    RequestPath = new PathString("/api/images")
});

app.UseCors(specificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();