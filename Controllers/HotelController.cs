using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using SmartHotelBooking.DbContext;
using SmartHotelBooking.Helpers;
using SmartHotelBooking.Models;

namespace SmartHotelBooking.Controllers

{

    public class HotelController(ApplicationDbContext dbContext) : Controller

    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        [Authorize(Policy = "MustBeAnAdmin")]

        public async Task<IActionResult> ListOfHotels()

        {

            var hotel = await _dbContext.Hotels

                .Include(h => h.Rooms)

                .ToListAsync();

            return View(hotel);

        }
        [Authorize(Policy = "MustBeAnMember")]
        public async Task<IActionResult> ListOfForMemberHotels()

        {

            var hotel = await _dbContext.Hotels

                .Include(h => h.Rooms)

                .ToListAsync();

            return View(hotel);

        }

        [Authorize(Policy = "MustBeAnHotelManager")]

        public async Task<IActionResult> ListOfManagerHotels()

        {

            int UserId = HttpContext.Session.GetInt32("UserId") ?? UserHelper.GetUserIdFromClaims(User);

            var hotels = await _dbContext.Hotels

                .Where(h => h.HotelManagerId == UserId)

                .ToListAsync();

            return View(hotels);

        }

        [Authorize(Policy = "AdminOrMember")]

        public async Task<IActionResult> HotelDetails(int HotelId)

        {

            int UserId = HttpContext.Session.GetInt32("UserId") ?? UserHelper.GetUserIdFromClaims(User);

            var user = await _dbContext.RegisterUser.FindAsync(UserId);

            if (user != null)

            {

                ViewData["UserRole"] = user.Role;

            }

            var hotel = await _dbContext.Hotels

                .Include(h => h.Rooms)

                .FirstOrDefaultAsync(h => h.HotelId == HotelId);

            if (hotel == null)

            {

                return NotFound();

            }

            var reviews = await _dbContext.Reviews

                .Where(r => r.HotelId == HotelId)

                .ToListAsync();

            ViewData["Reviews"] = reviews;

            return View(hotel);

        }
        [Authorize(Policy = "MustBeAnHotelManager")]

        public async Task<IActionResult> ManagerHotelDetails(int HotelId)

        {

            var hotel = await _dbContext.Hotels

                .Include(h => h.Rooms)

                .FirstOrDefaultAsync(h => h.HotelId == HotelId);

            if (hotel == null)

            {

                return NotFound();

            }

            var reviews = await _dbContext.Reviews

                .Where(r => r.HotelId == HotelId)

                .ToListAsync();

            ViewData["Reviews"] = reviews;

            return View(hotel);

        }

        [Authorize(Policy = "MustBeAnHotelManager")]

        public IActionResult CreateHotel()

        {

            int UserId = HttpContext.Session.GetInt32("UserId") ?? UserHelper.GetUserIdFromClaims(User);

            if (_dbContext.RegisterUser.Find(UserId) == null)

            {

                return RedirectToAction("AccessDenied", "User");

            }

            var hotel = new Hotel

            {

                HotelManagerId = UserId

            };

            return View(hotel);

        }

        [Authorize(Policy = "MustBeAnHotelManager")]

        [HttpPost]

        public async Task<IActionResult> CreateHotel(Hotel hotel)

        {

            if (ModelState.IsValid)

            {

                try

                {

                    await _dbContext.Hotels.AddAsync(hotel);

                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction("ListOfManagerHotels");

                }

                catch (Exception ex)

                {

                    ModelState.AddModelError("", $"Error: {ex.Message}");

                }

            }

            return View(hotel);

        }

        [Authorize(Policy = "MustBeAnHotelManager")]

        public async Task<IActionResult> EditHotel(int HotelId)

        {

            if (!ModelState.IsValid)

            {

                return NotFound();

            }

            var hotel = await _dbContext.Hotels.FindAsync(HotelId);

            if (hotel == null)

            {

                return NotFound();

            }

            return View(hotel);

        }

        [Authorize(Policy = "MustBeAnHotelManager")]

        [HttpPost]

        public async Task<IActionResult> EditHotel(Hotel hotel)

        {

            if (ModelState.IsValid)

            {

                try

                {

                    _dbContext.Hotels.Update(hotel);

                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction("ListOfManagerHotels");

                }

                catch (Exception ex)

                {

                    ModelState.AddModelError("", $"Error: {ex.Message}");

                }

            }

            return View(hotel);

        }

        [Authorize(Policy = "MustBeAnHotelManager")]

        public async Task<IActionResult> DeleteHotel(int HotelId)

        {

            if (!ModelState.IsValid)

            {

                return NotFound();

            }

            var hotel = await _dbContext.Hotels.FindAsync(HotelId);

            if (hotel == null)

            {

                return NotFound();

            }

            _dbContext.Hotels.Remove(hotel);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ListOfManagerHotels");

        }

        [Authorize(Policy = "MustBeAnMember")]

        public async Task<IActionResult> ViewRooms(int HotelId)

        {

            if (!ModelState.IsValid)

            {

                return NotFound();

            }

            var hotel = await _dbContext.Hotels

                .Include(h => h.Rooms)

                .FirstOrDefaultAsync(h => h.HotelId == HotelId);

            if (hotel == null)

            {

                return NotFound();

            }

            return View(hotel.Rooms);

        }
        [Authorize(Policy = "MustBeAnHotelManager")]
        public async Task<IActionResult> ManagerViewRooms(int HotelId)

        {

            if (!ModelState.IsValid)

            {

                return NotFound();

            }

            var hotel = await _dbContext.Hotels

                .Include(h => h.Rooms)

                .FirstOrDefaultAsync(h => h.HotelId == HotelId);

            if (hotel == null)

            {

                return NotFound();

            }

            return View(hotel.Rooms);

        }

    }

}

