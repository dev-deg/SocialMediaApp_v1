using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using SocialMediaApp_v1.DataAccess;
using SocialMediaApp_v1.Interfaces;
using SocialMediaApp_v1.Services;

try
{
    //Step 1: Build the application
    var builder = WebApplication.CreateBuilder(args);

    // Set Google credential path
    var credentialsPath = builder.Configuration["Authentication:Google:ServiceAccountCredentials"];
    Console.WriteLine($"Setting Google credentials path: {credentialsPath ?? "Not configured"}");
    if (!string.IsNullOrEmpty(credentialsPath))
    {
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
    }
    
    //setup the cloud logging service
    var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug());
    var cloudLoggingService = new CloudLoggingService(
        builder.Configuration, 
        loggerFactory.CreateLogger<CloudLoggingService>(),
        builder.Environment);
    
    builder.Services.AddSingleton<ICloudLoggingService>(cloudLoggingService);
    

    Console.WriteLine("Starting application initialization...");

    // Register secret manager service
    builder.Services.AddSingleton<ISecretManagerService, SecretManagerService>();

    // Load secrets
    var secretManager = new SecretManagerService(cloudLoggingService);
    await secretManager.LoadSecretsAsync(builder.Configuration).ConfigureAwait(false);
    

    //Step 3: Use the secrets to finalize the application configuration
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddGoogle(options =>
    {
        var clientId = builder.Configuration["Authentication:Google:ClientId"];
        var clientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        
        Console.WriteLine($"Google Auth - ClientId configured: {!string.IsNullOrEmpty(clientId)}");
        Console.WriteLine($"Google Auth - ClientSecret configured: {!string.IsNullOrEmpty(clientSecret)}");
        
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
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

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    else
    {
        // In development, show detailed errors
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    Console.WriteLine("Application configured successfully. Starting web server...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"FATAL ERROR: Application failed to start: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    
    // If there's an inner exception, log that too
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        Console.WriteLine(ex.InnerException.StackTrace);
    }
}
