using Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var appSettings = builder.Configuration.GetSection("AppSettings");
AppConfiguration.BrowseFolderRecursively = bool.Parse(appSettings["RecursiveBrowsingDirectory"]?.ToLower() ?? "true");
AppConfiguration.SnapshotDirectoryPath = appSettings["SnapshotPath"] ?? "Data/snapshot.json";

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.MapStaticAssets();

app.MapControllers();


app.Run();
