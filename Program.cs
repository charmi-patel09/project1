using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(JsonCrudApp.SharedResource));
    });

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
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("hi"),
    new CultureInfo("gu"),
    new CultureInfo("mr"),
    new CultureInfo("fr"),
    new CultureInfo("es"),
    new CultureInfo("de"),
    new CultureInfo("ar"),
    new CultureInfo("zh")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    ApplyCurrentCultureToResponseHeaders = true
};

app.UseRequestLocalization(localizationOptions);



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


