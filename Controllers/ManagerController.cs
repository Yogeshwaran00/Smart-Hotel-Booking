using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartHotelBooking.Controllers
{
    [Authorize(Policy = "MustBeAnHotelManager")]
    public class ManagerController : Controller
    {
        public IActionResult ManagerHome()
        {
            return View();
        }
        //list owned hotels  =>                   list of rooms =>                          list of bookings =>         list of reviews =>     get user regist =>    logout
        //add hotel => edit hotel => delete hotel // add room => edit room => delete room    //cancel booking           //delete review        //view user details
    }
}
