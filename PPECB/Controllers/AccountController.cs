using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PPECB.Domain.Entities;

namespace PPECB.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register()
    {
        _logger.LogInformation("Register GET page accessed");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string email, string password, string confirmPassword)
    {
        _logger.LogInformation($"Register POST attempt for email: {email}");

        // Check if passwords match
        if (password != confirmPassword)
        {
            _logger.LogWarning($"Password mismatch for email: {email}");
            ModelState.AddModelError("", "Passwords do not match");
            return View();
        }

        // Check if email is valid
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            _logger.LogWarning($"Invalid email format: {email}");
            ModelState.AddModelError("", "Valid email is required");
            return View();
        }

        // Check password strength
        if (string.IsNullOrEmpty(password) || password.Length < 6)
        {
            _logger.LogWarning($"Password too short for email: {email}");
            ModelState.AddModelError("", "Password must be at least 6 characters");
            return View();
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation($"Creating user: {email}");
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            _logger.LogInformation($"User created successfully: {email}");

            _logger.LogInformation($"Signing in user: {email}");
            await _signInManager.SignInAsync(user, isPersistent: false);

            _logger.LogInformation($"Redirecting to home page");
            return RedirectToAction("Index", "Home");
        }

        _logger.LogError($"User creation failed for {email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View();
    }

    [HttpGet]
    public IActionResult Login()
    {
        _logger.LogInformation("Login GET page accessed");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        _logger.LogInformation($"Login POST attempt for email: {email}");

        // Check if fields are empty
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            _logger.LogWarning($"Login failed - empty fields for email: {email}");
            ModelState.AddModelError("", "Email and password are required");
            return View();
        }

        _logger.LogInformation($"Attempting password sign in for: {email}");
        var result = await _signInManager.PasswordSignInAsync(email, password, false, false);

        if (result.Succeeded)
        {
            _logger.LogInformation($"User {email} logged in successfully");
            return RedirectToLocal(returnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning($"User {email} is locked out");
            ModelState.AddModelError("", "Account locked out. Try again later.");
        }
        else if (result.IsNotAllowed)
        {
            _logger.LogWarning($"User {email} not allowed to login (email not confirmed?)");
            ModelState.AddModelError("", "Login not allowed. Please confirm your email.");
        }
        else
        {
            _logger.LogWarning($"Invalid login attempt for {email}. Result: {result}");
            ModelState.AddModelError("", "Invalid login attempt");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);
        var email = user?.Email ?? "unknown";
        _logger.LogInformation($"User {email} logging out");

        await _signInManager.SignOutAsync();
        _logger.LogInformation($"User {email} logged out successfully");

        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}