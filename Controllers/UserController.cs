using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.EntityFrameworkCore;

using SmartHotelBooking.DbContext;
using SmartHotelBooking.Helpers;
using SmartHotelBooking.Models;

namespace SmartHotelBooking.Controllers

{

    public class UserController : Controller

    {

        private readonly UserManager<AccountUser> _userManager;
        private readonly SignInManager<AccountUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;
        public UserController(UserManager<AccountUser> userManager, SignInManager<AccountUser> signInManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        public IActionResult Login()

        {

            return View(new LoginUser());

        }

        [HttpPost]

        public async Task<IActionResult> Login(LoginUser loginUser)

        {

            if (!ModelState.IsValid)

            {

                return View(loginUser);

            }

            var accountUser = await _userManager.FindByEmailAsync(loginUser.Email);

            if (accountUser == null)

            {

                ModelState.AddModelError("UserName", "Invalid username or password.");

                return View(loginUser);

            }

            var result = await _signInManager.CheckPasswordSignInAsync(accountUser, loginUser.Password, false);

            if (result.Succeeded)

            {

                var claims = await _userManager.GetClaimsAsync(accountUser);

                if (claims != null && claims.Count > 0)

                {

                    var user = _dbContext.RegisterUser.FirstOrDefault(x => x.Email == loginUser.Email);

                    if (user != null)

                    {

                        HttpContext.Session.SetInt32("UserId", user.RegisterId);

                        var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);

                        var principal = new ClaimsPrincipal(identity);

                        var properties = new AuthenticationProperties

                        {

                            IsPersistent = true,

                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)

                        };

                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, properties);

                        if (user.Role == "Manager")
                        {
                            return RedirectToAction("ManagerHome", "Manager");
                        }
                        else if (user.Role == "Member")
                        {
                            return RedirectToAction("MemberHome", "Member");
                        }
                        else if (user.Role == "Admin")
                        {
                            return RedirectToAction("AdminHome", "Admin");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }

                    }

                    else

                    {

                        ModelState.AddModelError("Email", "User not found in the database.");

                        return View(loginUser);

                    }

                }

                else

                {

                    ModelState.AddModelError("Email", "Email not registered.");

                    return View(loginUser);

                }

            }

            else

            {

                ModelState.AddModelError("Password", "Password is incorrect.");

                return View(loginUser);

            }

        }

        public IActionResult RegisterUser()

        {

            var roles = new List<SelectListItem>

            {

                new SelectListItem { Value = "Manager", Text = "Manager" },

                new SelectListItem { Value = "Member", Text = "GuestUser" }

                //new SelectListItem { Value = "Admin", Text = "Admin" }

            };

            ViewData["Roles"] = roles;

            return View(new RegisterUser());

        }

        [HttpPost]

        public async Task<IActionResult> RegisterUser(RegisterUser user)

        {

            if (!ModelState.IsValid)

            {

                return View(user);

            }

            var newUser = new AccountUser

            {

                UserName = user.Email,

                Email = user.Email,

                FirstName = user.FirstName,

                LastName = user.LastName,

                PhoneNumber = user.PhoneNumber,

                Address = user.Address

            };

            var accountCreation = await _userManager.CreateAsync(newUser, user.Password);

            if (accountCreation.Succeeded)

            {

                var claims = new List<Claim>

                {

                    new (ClaimTypes.NameIdentifier, newUser.Id),

                    new (ClaimTypes.Name, newUser.UserName),

                    new (ClaimTypes.Email, newUser.Email),

                    new ("Role", user.Role)

                };

                var addClaimResult = await _userManager.AddClaimsAsync(newUser, claims);

                if (addClaimResult.Succeeded)

                {

                    // Hash the password using UserManager

                    var hashedPassword = _userManager.PasswordHasher.HashPassword(newUser, user.Password);

                    // Save the user to the custom RegisterUser table with the hashed password

                    var registerUser = new RegisterUser

                    {

                        FirstName = user.FirstName,

                        LastName = user.LastName,

                        Email = user.Email,

                        ConfirmEmailId = user.ConfirmEmailId,

                        Password = hashedPassword, // Store the hashed password

                        ConfirmPassword = hashedPassword, // Store the hashed password

                        PhoneNumber = user.PhoneNumber,

                        Address = user.Address,

                        Role = user.Role,

                        RegisterId = user.RegisterId

                    };

                    _dbContext.RegisterUser.Add(registerUser);

                    await _dbContext.SaveChangesAsync();

                    await _userManager.UpdateAsync(newUser);

                    return View("AccountCreated");

                }

            }

            ModelState.AddModelError(string.Empty, "Failed to create account.");

            return View(user);

        }

        public async Task<IActionResult> Logout()

        {

            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");

        }

        [Authorize(Policy = "MustBeAnHotelManager")]
        public async Task<IActionResult> UserDetails(string? searchUser)
        {
            int managerId = HttpContext.Session.GetInt32("UserId") ?? UserHelper.GetUserIdFromClaims(User);

            if (managerId == 0)
            {
                return RedirectToAction("AccessDenied", "User");
            }

            // Fetch users associated with hotels managed by the logged-in manager through bookings
            var usersQuery = _dbContext.RegisterUser
                .Where(u => _dbContext.Bookings
                    .Any(b => b.RegisterId == u.RegisterId && _dbContext.Hotels
                        .Any(h => h.HotelManagerId == managerId && h.HotelId == b.Room.HotelId)))
                .AsQueryable();

            // Filter by search query if provided
            if (!string.IsNullOrEmpty(searchUser))
            {
                usersQuery = usersQuery.Where(u => u.FirstName.Contains(searchUser) || u.LastName.Contains(searchUser) || u.Email.Contains(searchUser));
                ViewData["SearchUser"] = searchUser; // Pass the search query back to the view
            }

            var users = await usersQuery.ToListAsync();
            return View(users);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

    }

}

