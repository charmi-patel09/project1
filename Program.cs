using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

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
builder.Services.AddHttpClient<JsonCrudApp.Services.GlobalTranslationService>();
builder.Services.AddTransient<JsonCrudApp.Services.TimeTrackerService>();
builder.Services.AddTransient<JsonCrudApp.Services.HabitService>();
builder.Services.AddTransient<JsonCrudApp.Services.EmergencyService>();

builder.Services.AddTransient<JsonCrudApp.Services.UserActivityService>();
builder.Services.AddTransient<JsonCrudApp.Services.UserPdfService>();
builder.Services.AddTransient<JsonCrudApp.Services.GoalService>();

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

var supportedCultures = new[] { "en", "hi", "gu" };
var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseRouting();
app.UseSession();



app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Default to Login

// Bootstrap Initial Admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var studentService = services.GetRequiredService<JsonCrudApp.Services.JsonFileStudentService>();
        var authService = services.GetRequiredService<JsonCrudApp.Services.AuthService>();

        var students = studentService.GetStudents();
        // Check if any admin exists
        if (!students.Any(s => s.Role == "Admin"))
        {
            // Create default Super Admin
            // Email: admin@local.com, Password: AdminPassword123!
            authService.RegisterStudent(
                "admin@local.com",
                "AdminPassword123!",
                "Super Admin",
                99,
                "System",
                "Admin"
            );
            Console.WriteLine("Bootstrap: Default Super Admin created (admin@local.com).");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Bootstrap Error: {ex.Message}");
    }
}

app.Run();


