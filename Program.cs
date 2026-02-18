using ClientManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register DbContext with SQL Server
builder.Services.AddDbContext<ClientManagementSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Detailed errors in development
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Custom error page in production
    app.UseHsts(); // Adds HTTP Strict Transport Security in production
}

app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Default route points to ClientsController Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Clients}/{action=Index}/{id?}");

app.Run();