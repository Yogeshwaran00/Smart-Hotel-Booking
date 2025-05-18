using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartHotelBooking.Models;

namespace SmartHotelBooking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Retrieve the user's role from the claims
                var userRole = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;

                // Redirect based on the user's role
                if (userRole == "Admin")
                {
                    return RedirectToAction("AdminHome", "Admin");
                }
                else if (userRole == "Manager")
                {
                    return RedirectToAction("ManagerHome", "Manager");
                }
                else if (userRole == "Member")
                {
                    return RedirectToAction("MemberHome", "Member");
                }
            }

            // If the user is not authenticated or no role is found, show the default home page

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
