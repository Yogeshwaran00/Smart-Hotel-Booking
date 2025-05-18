using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.EntityFrameworkCore;

using SmartHotelBooking.DbContext;

using SmartHotelBooking.Helpers;

using SmartHotelBooking.Models;

namespace SmartHotelBooking.Controllers

{

    [Authorize(Policy = "MustBeAnAdmin")]

    public class AdminController(ApplicationDbContext dbContext) : Controller

    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        [Authorize(Policy = "MustBeAnAdmin")]

        public IActionResult AdminHome()

        {

            return View();

        }

        [Authorize(Policy = "MustBeAnAdmin")]

        public IActionResult AdminCreateHotel()

        {

            // Fetch the list of managers

            ViewBag.Managers = _dbContext.RegisterUser

                .Where(u => u.Role == "Manager")

                .Select(u => new SelectListItem

                {

                    Value = u.RegisterId.ToString(),

                    Text = u.FirstName + " " + u.LastName

                })

                .ToList();

            return View(new Hotel());

        }

        [Authorize(Policy = "MustBeAnAdmin")]

        [HttpPost]

        public async Task<IActionResult> AdminCreateHotel(Hotel hotel)

        {

            if (ModelState.IsValid)

            {

                try

                {

                    await _dbContext.Hotels.AddAsync(hotel);

                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction("ListOfHotels", "Hotel");

                }

                catch (Exception ex)

                {

                    ModelState.AddModelError("", $"Error: {ex.Message}");

                }

            }

            // Re-fetch the list of managers in case of validation errors

            ViewBag.Managers = _dbContext.RegisterUser

                .Where(u => u.Role == "Manager")

                .Select(u => new SelectListItem

                {

                    Value = u.RegisterId.ToString(),

                    Text = u.FirstName + " " + u.LastName

                })

                .ToList();

            return View(hotel);

        }


        [Authorize(Policy = "MustBeAnAdmin")]

        public async Task<IActionResult> AdminEditHotel(int HotelId)

        {

            if (!ModelState.IsValid)

            {

                return NotFound();

            }

            var hotel = await _dbContext.Hotels.FindAsync(HotelId);

            ViewBag.Managers = _dbContext.RegisterUser

    .Where(u => u.Role == "Manager")

    .Select(u => new SelectListItem

    {

        Value = u.RegisterId.ToString(),

        Text = u.FirstName + " " + u.LastName

    })

    .ToList();

            if (hotel == null)

            {

                return NotFound();

            }

            return View(hotel);

        }

        [Authorize(Policy = "MustBeAnAdmin")]

        [HttpPost]

        public async Task<IActionResult> AdminEditHotel(Hotel hotel)

        {

            if (ModelState.IsValid)

            {

                try

                {

                    _dbContext.Hotels.Update(hotel);

                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction("ListOfHotels", "Hotel");

                }

                catch (Exception ex)

                {

                    ModelState.AddModelError("", $"Error: {ex.Message}");

                }

            }

            ViewBag.Managers = _dbContext.RegisterUser

    .Where(u => u.Role == "Manager")

    .Select(u => new SelectListItem

    {

        Value = u.RegisterId.ToString(),

        Text = u.FirstName + " " + u.LastName

    })

    .ToList();

            return View(hotel);

        }

        [Authorize(Policy = "MustBeAnAdmin")]

        public async Task<IActionResult> AdminDeleteHotel(int HotelId)

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

            return RedirectToAction("ListOfHotels", "Hotel");

        }

        [Authorize(Policy = "MustBeAnAdmin")]

        public async Task<IActionResult> AdminViewRooms(int HotelId)

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

        [Authorize(Policy = "MustBeAnAdmin")]

        public async Task<IActionResult> ListOfUsers(string? searchQuery)

        {

            // Fetch all users

            var usersQuery = _dbContext.RegisterUser.AsQueryable();

            // Filter users based on the search query

            if (!string.IsNullOrEmpty(searchQuery))

            {

                usersQuery = usersQuery.Where(u =>

                    u.FirstName.Contains(searchQuery) ||

                    u.LastName.Contains(searchQuery) ||

                    u.Email.Contains(searchQuery) ||

                    u.Role.Contains(searchQuery));

                ViewBag.SearchQuery = searchQuery; // Pass the search query back to the view

            }

            var users = await usersQuery.ToListAsync();

            return View(users);

        }

        [Authorize(Policy = "MustBeAnAdmin")]

        public async Task<IActionResult> DeleteUsers(int UserId)

        {

            if (!ModelState.IsValid)

            {

                return NotFound();

            }

            var user = await _dbContext.RegisterUser.FindAsync(UserId);

            if (user == null)

            {

                return NotFound();

            }

            _dbContext.RegisterUser.Remove(user);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ListOfUsers");

        }

        //lisofhotels => display all room => list of booings => delete hotel / delete manager / delete user => all review => logout

    }

}
