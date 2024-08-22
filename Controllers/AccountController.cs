using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PhuKienShop.Data;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.AspNetCore.Authorization;

namespace PhuKienShop.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly PkShopContext _context;
        private readonly ILogger<AccountController> _logger;
        public AccountController(PkShopContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult MyAccount()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var user = _context.Users.FirstOrDefault(u => u.Email == email);

                if (role != null && user != null)
                {
                    if (role == "Admin") // Kiểm tra nếu người dùng là Admin
                    {
                        return RedirectToAction("Index", "ManageUsers"); // Chuyển hướng đến trang quản lý của admin
                    }
                    else
                    {
                        var orders = _context.Orders
                     .Where(o => o.UserId == user.UserId) // Hoặc `o.UserId == user.Id` nếu bạn có thuộc tính `UserId`
                     .ToList();

                        var viewModel = new MyAccountViewModel
                        {
                            User = user,
                            Orders = orders,
                            UserPhotoUrl = ViewData["UserPhotoUrl"]?.ToString()
                        };

                        return View(viewModel);
                    }

                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile userPhoto)
        {
            if (userPhoto != null && userPhoto.Length > 0)
            {
                // Đường dẫn đến thư mục UserImg
                var userImgFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "UserImg");

                if (!Directory.Exists(userImgFolderPath))
                {
                    Directory.CreateDirectory(userImgFolderPath);
                }

                // Lưu ảnh vào thư mục UserImg
                var filePath = Path.Combine(userImgFolderPath, userPhoto.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await userPhoto.CopyToAsync(stream);
                }

                var fileUrl = Url.Action("GetUserImage", "File", new { filename = userPhoto.FileName });

                TempData["UserPhotoUrl"] = fileUrl;

                TempData["Message"] = "Ảnh đã được tải lên thành công!";
            }
            else
            {
                TempData["Message"] = "Vui lòng chọn ảnh để tải lên.";
            }

            return RedirectToAction("MyAccount");
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
        // GET: Account/UpdateAccount
        [HttpGet]
        public IActionResult UpdateAccount()
        {
            if (User.Identity.IsAuthenticated)
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var userDetails = _context.Users.FirstOrDefault(u => u.Email == email);

                if (userDetails != null)
                {
                    return View(userDetails);
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // POST: Account/UpdateAccount
        [HttpPost]
        public IActionResult UpdateAccount(User updatedUser)
        {
            if (User.Identity.IsAuthenticated)
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var user = _context.Users.FirstOrDefault(u => u.Email == email);

                if (user != null)
                {
                    // Update user details
                    user.FullName = updatedUser.FullName;
                    user.PhoneNumber = updatedUser.PhoneNumber;
                    user.Address = updatedUser.Address;
                    user.UpdatedAt = DateTime.Now;
                    _context.Users.Update(user);
                    _context.SaveChanges();

                    return RedirectToAction("MyAccount");
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email không tồn tại trong hệ thống.");
                return View();
            }

            var resetCode = new Random().Next(100000, 999999).ToString(); // Tạo mã xác thực 6 chữ số
            TempData["ResetCode"] = resetCode;
            TempData["VerifiedEmail"] = email;
            

            // Gửi email mã xác thực
            await SendEmailAsync(email, "Password Reset", $"Your password reset code is: {resetCode}");

            return RedirectToAction("EnterResetCode");
        }
        [HttpGet]
        public IActionResult EnterResetCode()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EnterResetCode(string code)
        {
            var expectedCode = TempData["ResetCode"] as string;
            var email = TempData["VerifiedEmail"] as string;

            if (expectedCode != null && expectedCode == code)
            {
                TempData["VerifiedEmail"] = email;
                return RedirectToAction("ResetPassword");
            }
            // Reassign email back to TempData even if the code is incorrect
            TempData["VerifiedEmail"] = email;
            TempData["ResetCode"] = expectedCode;

            ModelState.AddModelError("", "Mã xác thực không hợp lệ.");
            return View();
        }
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["VerifiedEmail"] == null)
            {
                
                
                return RedirectToAction("ForgotPassword");
            }
            var email = TempData["VerifiedEmail"] as string;
            TempData["VerifiedEmail"] = email;
            return View();
        }
        [HttpPost]
        public IActionResult ResetPassword(string newPassword, string confirmPassword)
        {
            var email = TempData["VerifiedEmail"] as string;

            if (email == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            // Reassign the email back to TempData to keep it across multiple requests
            TempData["VerifiedEmail"] = email;

            // Validate the new password
            if (newPassword.Length < 8 || !newPassword.Any(char.IsDigit) || !newPassword.Any(char.IsLetter))
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới phải có ít nhất 8 kí tự bao gồm cả số và chữ.");
                return View();
            }

            // Validate if new password and confirm password match
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới và mật khẩu xác nhận không khớp.");
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.Password = hashedPassword;
                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                // Clear TempData after success
                TempData.Remove("VerifiedEmail");
                TempData.Remove("ResetCode");

                TempData["SuccessMessage"] = "Mật khẩu đã được đặt lại thành công.";
                return RedirectToAction("Login");
            }

            return RedirectToAction("Error", "Home");
        }





    }
}