
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using SocialMediaApp_v1.DataAccess;
using SocialMediaApp_v1.Interfaces;
using SocialMediaApp_v1.Services;

//Step 1: Build the application
var builder = WebApplication.CreateBuilder(args);

Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", builder.Configuration["Authentication:Google:ServiceAccountCredentials"]);


var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole();
});
var logger = loggerFactory.CreateLogger<SecretManagerService>();
var secretService = new SecretManagerService(logger);
await secretService.LoadSecretsAsync(builder.Configuration);

//Step 3: Use the secrets to finalise the application configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    //Extracting the user profile information and updating the Identity Claims
    options.Scope.Add("profile");
    options.Events.OnCreatingTicket = (context) =>
    {
        String email = context.User.GetProperty("email").GetString();
        String picture = context.User.GetProperty("picture").GetString();
        context.Identity.AddClaim(new Claim("Email", email));
        context.Identity.AddClaim(new Claim("Picture", picture));
        return Task.CompletedTask;
    };
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<FirestoreRepository>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<ISecretManagerService, SecretManagerService>();

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