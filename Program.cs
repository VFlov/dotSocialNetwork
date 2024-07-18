using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using dotSocialNetwork.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using dotSocialNetwork.Repository;
using dotSocialNetwork.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<dotSocialNetworkContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dotSocialNetworkContext") ?? throw new InvalidOperationException("Connection string 'dotSocialNetworkContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/Login");
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRepository<Friend>, FriendsRepository>();
builder.Services.AddScoped<IRepository<Message>, MessageRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
