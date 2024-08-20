using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PhuKienShop.Data;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace PhuKienShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly PkShopContext _context;
        private readonly ILogger<AccountController> _logger;
        public AccountController(PkShopContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public IActionResult MyAccount()
        {
            if (User.Identity.IsAuthenticated)
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                if (role != null)
                {
                    if (role == "Admin")
                    {
                        return RedirectToAction("Index", "AdminMessages");
                    }
                    else
                    {
                        var userDetails = _context.Users.FirstOrDefault(u => u.Email == email);

                        if (userDetails != null)
                        {
                            return View("MyAccount", userDetails);
                        }
                        else
                        {
                            return RedirectToAction("Error", "Home");
                        }
                    }
                }
                else
                {
                    // Handle case where role is null or unrecognized
                    return RedirectToAction("Error", "Home");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }



        [HttpGet]
        public IActionResult Register()
        {
            return View("Register");
        }
        [HttpPost]
        public JsonResult CheckEmail(string email)
        {
            var userExists = _context.Users.Any(u => u.Email == email);
            return Json(new { exists = userExists });
        }
        [HttpPost]
        public JsonResult CheckUsername(string username)
        {
            var userExists = _context.Users.Any(u => u.Username == username);
            return Json(new { exists = userExists });
        }


        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            // Check fomart email
            // Validate email format
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
            }

            // Check if the email already exists
            if (_context.Users.Any(u => u.Email == email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng!");
                    return View();
                }

                // Check if the username already exists
                if (_context.Users.Any(u => u.Username == username))
                {
                    ModelState.AddModelError("Username", "Username đã được sử dụng!");
                    return View();
                }
            if (ModelState.IsValid)
            {
                // Proceed with the registration
                var verificationCode = new Random().Next(100000, 999999).ToString(); // Generate a 6-digit verification code
                TempData["VerificationCode"] = verificationCode;
                TempData["Email"] = email;
                TempData["Username"] = username;
                TempData["Password"] = BCrypt.Net.BCrypt.HashPassword(password); // Hash the password

                // Send email verification code
                await SendEmailAsync(email, "Verify your email", $"Your verification code is: {verificationCode}");

                return View("EnterVerificationCode");
            }

            // If we got this far, something failed; redisplay the form with errors
            return View("Register");
        }


        [HttpPost]
    public IActionResult VerifyCode(string code)
{
    var expectedCode = TempData["VerificationCode"] as string;
    var email = TempData["Email"] as string;
    var username = TempData["Username"] as string;
    var hashedPassword = TempData["Password"] as string;

    if (expectedCode != null && expectedCode == code)
    {
        var user = new User
        {
            Username = username,
            Email = email,
            Password = hashedPassword,
            Role = "User",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return RedirectToAction("Login", "Account");
    }

    ModelState.AddModelError("", "Mã xác thực không hợp lệ!.");
    return View("EnterVerificationCode");
}

		private async Task SendEmailAsync(string to, string subject, string body)
		{
			var fromAddress = new MailAddress("anzorobd@gmail.com", "Thái Tường An");
            var toAddress = new MailAddress(to);
			var message = new MailMessage
			{
                Subject = subject,
				From = fromAddress,
				Body = body,
				IsBodyHtml = true
			};
			message.To.Add(toAddress);

			using (var smtp = new SmtpClient())
			{
				smtp.Host = "smtp.gmail.com";
				smtp.Port = 587;
				smtp.Credentials = new NetworkCredential("anzorobd@gmail.com", "wzud casb guyv wsho");
				smtp.EnableSsl = true;
				await smtp.SendMailAsync(message);
			}
		}
		[HttpGet]
        public IActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("User with email {Email} not found.", email);
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View();
            }

            if (BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString() ?? string.Empty)
        };
                var claimsIdentity = new ClaimsIdentity(claims, "PhuKienShopAuth");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync("PhuKienShopAuth", claimsPrincipal);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                _logger.LogWarning("Password verification failed for user {Email}.", email);
                ModelState.AddModelError("", "Đăng nhập không thành công. Vui lòng kiểm tra lại email và mật khẩu.");
                return View();
            }
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
