using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartHotelBooking.DbContext;

using SmartHotelBooking.Helpers;

using SmartHotelBooking.Models;
//using Stripe.BillingPortal;
using Stripe.Checkout;

namespace SmartHotelBooking.Controllers

{

    public class BookingController(ApplicationDbContext dbContext,IOptions<StripeSettings> stripeSettings) : Controller

    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        private readonly StripeSettings _stripeSettings = stripeSettings.Value;
        private int userId;


        [Authorize(Policy = "MustBeAnMember")]

        public async Task<IActionResult> ListOfBookings(string? searchRoom)

        {

         

            int userId = HttpContext.Session.GetInt32("UserId") ?? UserHelper.GetUserIdFromClaims(User);

           

            var bookingsQuery = _dbContext.Bookings

                .Include(b => b.Room) 

                .Where(b => b.RegisterId == userId) 

                .AsQueryable();


            if (!string.IsNullOrEmpty(searchRoom))

            {

                bookingsQuery = bookingsQuery.Where(b => b.RoomId.ToString().Contains(searchRoom));

                ViewData["SearchRoom"] = searchRoom; 

            }

            var bookings = await bookingsQuery.ToListAsync();

            return View(bookings);

        }

        

        [Authorize(Policy = "MustBeAnAdmin")]

        public async Task<IActionResult> ListAllBookingsForAdmin(string? searchHotel)

        {

           

            var bookingsQuery = _dbContext.Bookings

                .Include(b => b.Room)

                .ThenInclude(r => r.Hotel) 

                .AsQueryable();

           

            if (!string.IsNullOrEmpty(searchHotel))

            {

                bookingsQuery = bookingsQuery.Where(b => b.Room != null && b.Room.Hotel != null && b.Room.Hotel.HotelName != null && b.Room.Hotel.HotelName.Contains(searchHotel));

                ViewData["SearchHotel"] = searchHotel; 

            }

            var bookings = await bookingsQuery.ToListAsync();

            return View(bookings);

        }


        [Authorize(Policy = "MustBeAnHotelManager")]

        public async Task<IActionResult> ListBookingsForManager(string? searchHotel)

        {

            

            int managerId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (managerId == 0)

            {

                return RedirectToAction("AccessDenied", "User");

            }

          

            var bookingsQuery = _dbContext.Bookings

                .Include(b => b.Room)

                .ThenInclude(r => r.Hotel) 

                .Where(b => b.Room != null && b.Room.Hotel != null && b.Room.Hotel.HotelManagerId == managerId)

                .AsQueryable();

          

            if (!string.IsNullOrEmpty(searchHotel))

            {

                bookingsQuery = bookingsQuery.Where(b => b.Room.Hotel.HotelName != null && b.Room.Hotel.HotelName.Contains(searchHotel));

                ViewData["SearchHotel"] = searchHotel; // Pass the search query back to the view

            }

            var bookings = await bookingsQuery.ToListAsync();

            return View(bookings);

        }


        [Authorize(Policy = "MustBeAnMember")]

   

        public async Task<IActionResult> CreateBooking(int roomID)

        {
            
            
            int UserId = HttpContext.Session.GetInt32("UserId") ?? UserHelper.GetUserIdFromClaims(User);

            

            var room = await _dbContext.Rooms.FindAsync(roomID);

            if (room == null)

            {

                return RedirectToAction("AccessDenied", "User");

            }

         

            var user = await _dbContext.RegisterUser.FindAsync(UserId);

            if (user == null)

            {

                return RedirectToAction("AccessDenied", "User");

            }

           

            ViewBag.RedemptionPoints = user.RedemptionPoints;

         

            var booking = new Booking

            {

                RoomId = room.RoomId,

                RegisterId = user.RegisterId,

                RegisterUser = user 

            };

            return View(booking);

        }


        [Authorize(Policy = "MustBeAnMember")]

        

        [HttpPost]

        public async Task<IActionResult> CreateBooking(Booking booking)

        {

            if (ModelState.IsValid)
            {
                ViewBag.RoomId = booking.RoomId;
                ViewBag.RegisterId = booking.RegisterId;
                try
                { 
                   
                    var overlappingBooking = await _dbContext.Bookings
                        .Where(b => b.RoomId == booking.RoomId &&
                                    b.CheckOutDate > booking.CheckInDate && 
                                    b.CheckInDate < booking.CheckOutDate)  
                        .FirstOrDefaultAsync();
                    if (overlappingBooking != null)
                    {
                        ModelState.AddModelError("", "The room is already booked for the selected dates.");
                        return View(booking);
                    }
                    booking.Status = "Confirmed";
                    
                    var user = await _dbContext.RegisterUser.FindAsync(booking.RegisterId);
                    if (user != null)
                    {
                        user.BookedCount += 1; 
                        user.RedemptionPoints += 2; 
                        booking.PointsEarned = 2; 
                        
                        if (user.BookedCount > 10)
                        {
                            user.IsLoyalCustomer = true;
                        }
                        
                        if (user.RedemptionPoints >= 100)
                        {
                            booking.TotalPrice = 0; 
                            user.RedemptionPoints -= 100; 
                        }
                        else
                        {
                            
                            if (user.IsLoyalCustomer)
                            {
                                booking.TotalPrice *= 0.95; 
                            }
                        }
                        _dbContext.RegisterUser.Update(user); 
                    }
                    
                    var room = await _dbContext.Rooms.FindAsync(booking.RoomId);
                    if (room != null)
                    {
                        var duration = (booking.CheckOutDate - booking.CheckInDate).TotalDays;
                        booking.TotalPrice = duration * room.Price;
                        
                        room.IsAvailable = false;
                        _dbContext.Rooms.Update(room);
                    }
                    
                    await _dbContext.Bookings.AddAsync(booking);
                    await _dbContext.SaveChangesAsync();
                    
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay((int)(booking.CheckOutDate - DateTime.Now).TotalMilliseconds);
                        var bookedRoom = await _dbContext.Rooms.FindAsync(booking.RoomId);
                        if (bookedRoom != null)
                        {
                            bookedRoom.IsAvailable = true;
                            _dbContext.Rooms.Update(bookedRoom);
                            await _dbContext.SaveChangesAsync();
                        }
                    });

                    //return RedirectToAction("MakePayment", "Payment");
                    return RedirectToAction("ListOfBookings");
                }
                catch (Exception ex)
                {
                    
                    var room = await _dbContext.Rooms.FindAsync(booking.RoomId);
                    if (room != null)
                    {
                        room.IsAvailable = true;
                    }
                    booking.Status = "Cancelled";
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                    return View(booking);
                }
            }
            return View(booking);
        }

        //[HttpPost]
        //public IActionResult CreateBookingWithPayment(Booking booking)
        //{
        //    if (ModelState.IsValid)
        //    {

        //    }
        //    return View();
        //}

        [Authorize(Policy = "MustBeAnMember")]

        public async Task<IActionResult> CancelBooking(int BookingId)

        {

            if (!ModelState.IsValid)

            {

                return NotFound();

            }

            var booking = await _dbContext.Bookings

                .Include(b => b.Room) 

                .FirstOrDefaultAsync(b => b.BookingId == BookingId);

            if (booking == null)

            {

                return NotFound();

            }

            

            if ((booking.CheckInDate - DateTime.Now).TotalHours < 24)

            {

                ModelState.AddModelError("", "Cannot cancel a booking less than 24 hours before the check-in date.");

                return View(booking);

            }

           

            var user = await _dbContext.RegisterUser.FindAsync(booking.RegisterId);

            if (user != null)

            {

                user.BookedCount -= 1; 

                user.RedemptionPoints -= booking.PointsEarned; 

                if (user.RedemptionPoints < 0)

                {

                    user.RedemptionPoints = 0; 

                }

                _dbContext.RegisterUser.Update(user); 

            }

            if (booking.Room != null)

            {

                booking.Room.IsAvailable = true; 

                _dbContext.Rooms.Update(booking.Room);

            }

           

            _dbContext.Bookings.Remove(booking);

        

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ListOfBookings");

        }


    }

}

