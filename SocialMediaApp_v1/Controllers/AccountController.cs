using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;
using System.Threading.Tasks;

namespace SocialMediaApp_v1.Controllers;

public class AccountController : Controller
{
    //Initiate the login procedure, setting the redirect uri to GoogleResponse
    public IActionResult Login()
    {
        var authenticationProperties = new AuthenticationProperties
            { RedirectUri = Url.Action("GoogleResponse") };
        return Challenge(authenticationProperties, GoogleDefaults.AuthenticationScheme);
    }

    //The user is redirected here after successful login
    public async Task<IActionResult> GoogleResponse()
    {
        var authenticationResult =
            await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //If the login is successful, we redirect the user to the home page
        if (!authenticationResult.Succeeded)
            return RedirectToAction("Index", "Home");
        
        var claimsIdentity = new ClaimsIdentity(authenticationResult.Principal.Identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));
        return RedirectToAction("Index", "Home");
    }

    //Logs the user out by clearing the authentication cookie and redirects to home
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
    
}