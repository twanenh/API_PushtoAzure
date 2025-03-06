using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API_PushtoAzure.Controllers
{
    [Route("api/auth")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok(new { message = "Đăng ký thành công" });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, false);
            if (result.Succeeded)
            {
                // Đảm bảo cookie đã được thiết lập bởi Identity
                var user = await _userManager.FindByEmailAsync(model.Email);

                // Trả về thông tin để Angular lưu vào sessionStorage
                return Ok(new
                {
                    message = "Đăng nhập thành công",
                    email = user.Email,
                    userId = user.Id
                });
            }
            else
            {
                return Unauthorized("Sai tài khoản hoặc mật khẩu");
            }
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            // Đảm bảo cookie được xóa
            Response.Cookies.Delete(".AspNetCore.Identity.Application", new CookieOptions
            {
                SameSite = SameSiteMode.Lax,
                Secure = true
            });

            return Ok(new { message = "Đăng xuất thành công" });
        }
        [HttpGet("check-login")]
        public IActionResult CheckLoginStatus()
        {
            // Ghi log để debug
            Console.WriteLine($"User authenticated: {User.Identity.IsAuthenticated}");

            if (User.Identity.IsAuthenticated)
            {
                var email = User.Identity.Name; // ASP.NET Core Identity lưu email vào Name claim
                return Ok(new
                {
                    isAuthenticated = true,
                    email = email
                });
            }

            return Ok(new { isAuthenticated = false });
        }

    }
}
