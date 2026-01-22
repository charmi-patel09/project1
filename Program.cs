// using System.Globalization;
// using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddTransient<JsonCrudApp.Services.JsonFileStudentService>();
builder.Services.AddTransient<JsonCrudApp.Services.AuthService>();
builder.Services.AddTransient<JsonCrudApp.Services.EmailService>();
builder.Services.AddTransient<JsonCrudApp.Services.OtpService>();
builder.Services.AddTransient<JsonCrudApp.Services.NotesService>();
builder.Services.AddTransient<JsonCrudApp.Services.GlobalTranslationService>();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();



app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=SignUp}/{id?}");

app.Run();


