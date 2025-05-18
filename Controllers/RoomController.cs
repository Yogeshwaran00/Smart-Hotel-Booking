using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.EntityFrameworkCore;

using SmartHotelBooking.DbContext;
using SmartHotelBooking.Helpers;
using SmartHotelBooking.Models;

namespace SmartHotelBooking.Controllers

{

    public class RoomController(ApplicationDbContext dbContext) : Controller

    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        [Authorize(Policy = "MustBeAnHotelManager")]

        public async Task<IActionResult> ListOfRooms()

        {

            var rooms = await _dbContext.Rooms.Include(r => r.Booking).ToListAsync();

            foreach (var room in rooms)

            {

                if (room.Booking != null && room.Booking.CheckOutDate <= DateTime.Now)

                {

                    room.IsAvailable = true; // Mark the room as available

                    _dbContext.Rooms.Update(room);

                }

            }

            await _dbContext.SaveChangesAsync(); // Persist changes to the database

            return View(rooms);

        }

        [Authorize(Policy = "MustBeAnMember")]

        public async Task<IActionResult> RoomDetails(int roomId)

        {

            // Fetch the room and its associated bookings

            var room = await _dbContext.Rooms

                .Include(r => r.Booking) // Include associated bookings

                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)

            {

                return NotFound();

            }

            // Check if the room has any active bookings

            var activeBooking = await _dbContext.Bookings

                .Where(b => b.RoomId == roomId && b.CheckOutDate > DateTime.UtcNow) // Use UTC for consistency

                .FirstOrDefaultAsync();

            // Determine availability without updating the database

            room.IsAvailable = activeBooking == null; // Room is available if no active bookings exist

            return View(room); // Pass the room to the view

        }
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> ManagerRoomDetails(int roomId)

        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? UserHelper.GetUserIdFromClaims(User);

            var user = await _dbContext.RegisterUser.FindAsync(userId);

            if (user != null)

            {

                ViewData["UserRole"] = user.Role;

            }
            // Fetch the room and its associated bookings

            var room = await _dbContext.Rooms

                .Include(r => r.Booking) // Include associated bookings

                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)

            {

                return NotFound();

            }

            // Check if the room has any active bookings

            var activeBooking = await _dbContext.Bookings

                .Where(b => b.RoomId == roomId && b.CheckOutDate > DateTime.UtcNow) // Use UTC for consistency

                .FirstOrDefaultAsync();

            // Determine availability without updating the database

            room.IsAvailable = activeBooking == null; // Room is available if no active bookings exist

            return View(room); // Pass the room to the view

        }

        [Authorize(Policy = "MustBeAnHotelManager")]

        public async Task<IActionResult> CreateRoom()

        {

            var room = new Room();

            int managerId = HttpContext.Session.GetInt32("UserId") ?? UserHelper.GetUserIdFromClaims(User);

            // Filter hotels by the current manager's ID

            var hotels = await _dbContext.Hotels

                .Where(h => h.HotelManagerId == managerId)

                .ToListAsync();

            ViewBag.HotelId = new SelectList(hotels, "HotelId", "HotelName");

            return View(room);

        }

        [Authorize(Policy = "MustBeAnHotelManager")]

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> CreateRoom(Room room)

        {

            if (ModelState.IsValid)

            {

                try

                {

                    await _dbContext.Rooms.AddAsync(room);

                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction("ListOfManagerHotels","Hotel");

                }

                catch (Exception ex)

                {

                    ModelState.AddModelError("", $"Error: {ex.Message}");

                }

            }

            int managerId = HttpContext.Session.GetInt32("UserId") ?? UserHelper.GetUserIdFromClaims(User);

            // Filter hotels by the current manager's ID

            var hotels = await _dbContext.Hotels

                .Where(h => h.HotelManagerId == managerId)

                .ToListAsync();

            ViewBag.HotelId = new SelectList(hotels, "HotelId", "HotelName", room.HotelId);

            return View(room);

        }

        [Authorize(Policy = "MustBeAnHotelManager")]

        public async Task<IActionResult> EditRoom(int RoomId)

        {

            // Find the room by its ID

            var room = await _dbContext.Rooms.FindAsync(RoomId);

            if (room == null)

            {

                return NotFound();

            }

            // Fetch all hotels except the one associated with the current room

            var hotels = await _dbContext.Hotels

                .Where(h => h.HotelId == room.HotelId) // Exclude the current hotel

                .ToListAsync();

            // Populate the ViewBag with the filtered list of hotels

            ViewBag.HotelId = new SelectList(hotels, "HotelId", "HotelName", room.HotelId);

            return View(room);

        }

        [Authorize(Policy = "MustBeAnHotelManager")]

        [HttpPost]

        public async Task<IActionResult> EditRoom(Room room)

        {

            if (ModelState.IsValid)

            {

                try

                {

                    // Fetch the existing room from the database

                    var existingRoom = await _dbContext.Rooms.FindAsync(room.RoomId);

                    if (existingRoom == null)

                    {

                        return NotFound();

                    }

                    // Update the properties of the existing room

                    existingRoom.RoomType = room.RoomType;

                    existingRoom.Features = room.Features;

                    existingRoom.Price = room.Price;

                    existingRoom.Capacity = room.Capacity;

                    existingRoom.IsAvailable = room.IsAvailable;

                    existingRoom.HotelId = room.HotelId;

                    // Save changes to the database

                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction("ManagerViewRooms", "Hotel", new { HotelId = room.HotelId });

                }

                catch (Exception ex)

                {

                    ModelState.AddModelError("", $"Error: {ex.Message}");

                }

            }

            // Reload the list of hotels for the dropdown in case of an error

            ViewBag.HotelId = new SelectList(await _dbContext.Hotels.ToListAsync(), "HotelId", "HotelName", room.HotelId);

            return View(room);

        }


        [Authorize(Policy = "MustBeAnHotelManager")]

        public async Task<IActionResult> DeleteRoom(int RoomId)

        {

            if (!ModelState.IsValid)

            {

                return NotFound();

            }

            var room = await _dbContext.Rooms.FindAsync(RoomId);

            if (room == null)

            {

                return NotFound();

            }

            int hotelId = room.HotelId;

            _dbContext.Rooms.Remove(room);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ManagerViewRooms", "Hotel", new { HotelId = hotelId });

        }

    }

}
