using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PhuKienShop.Data;
using System.Security.Claims;

namespace PhuKienShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly PkShopContext _context;

        public AccountController(PkShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password)
        {
            if (_context.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("", "Email is already taken.");
                return View();
            }

            var user = new User
            {
                Username = username,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Automatically log in the user after registration
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, "PhuKienShopAuth");
            var principal = new ClaimsPrincipal(identity);

            HttpContext.SignInAsync("PhuKienShopAuth", principal);

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var identity = new ClaimsIdentity(claims, "PhuKienShopAuth");
                var principal = new ClaimsPrincipal(identity);

                HttpContext.SignInAsync("PhuKienShopAuth", principal);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync("PhuKienShopAuth");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse"),
                Items = { { "returnUrl", returnUrl } }
            };
            return Challenge(properties, "Google");
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var info = await HttpContext.AuthenticateAsync("Google");
            if (info == null)
                return RedirectToAction(nameof(Login));

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.SingleOrDefault(u => u.Email == email);
            if (user != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

                var identity = new ClaimsIdentity(claims, "PhuKienShopAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("PhuKienShopAuth", principal);

                return RedirectToAction("Index", "Home");
            }

            user = new User
            {
                Username = info.Principal.FindFirstValue(ClaimTypes.Email),
                Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                FullName = info.Principal.FindFirstValue(ClaimTypes.Name),
                Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Hoặc null nếu bạn đã thay đổi cột Password để cho phép null
                Role = "User",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var newClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var newIdentity = new ClaimsIdentity(newClaims, "PhuKienShopAuth");
            var newPrincipal = new ClaimsPrincipal(newIdentity);

            await HttpContext.SignInAsync("PhuKienShopAuth", newPrincipal);

            return RedirectToAction("Index", "Home");
        }

    }
}
