using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartHotelBooking.Controllers
{
    [Authorize(Policy="MustBeAnMember")]
    public class MemberController : Controller
    {
        public IActionResult MemberHome()
        {
            return View();
        }
        //lisofhotels => display all room => book room => booked details => return to list of hotels  => review 
        //view hotel =>   view room       => add book    => cancel book  => view hotels               => add review
        //home -> list of hotel -> header bar -> booked details -> all review -> logoout
    }
}
