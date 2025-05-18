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

    public class ReviewController(ApplicationDbContext dbContext, UserManager<AccountUser> userManager) : Controller

    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        private readonly UserManager<AccountUser> _userManager = userManager;
        [Authorize(Policy = "MustBeAnMember")]
        public async Task<IActionResult> ListOfReviews()

        {

            var reviews = await _dbContext.Reviews.ToListAsync();

            return View(reviews);

        }
        [Authorize(Policy ="MustBeAnAdmin")]
        public async Task<IActionResult> ListOfAdminReviews()

        {

            var reviews = await _dbContext.Reviews.ToListAsync();

            return View(reviews);

        }
        [Authorize(Policy = "MustBeAnMember")]
        public async Task<IActionResult> ListOfUserReviews(string? searchHotel)
        {
            // Get the current user's RegisterId from the session
            int userId = HttpContext.Session.GetInt32("UserId") ?? UserHelper.GetUserIdFromClaims(User);

            // Ensure the user is logged in
            if (userId == 0)
            {
                return RedirectToAction("AccessDenied", "User");
            }

            // Fetch reviews only for the logged-in user
            var reviewsQuery = _dbContext.Reviews
                .Where(r => r.RegisterId == userId) // Filter by the current user's RegisterId
                .AsQueryable();

            // Filter by hotel name if search query is provided
            if (!string.IsNullOrEmpty(searchHotel))
            {
                reviewsQuery = reviewsQuery.Where(r => r.HotelName != null && r.HotelName.Contains(searchHotel));
                ViewBag.SearchHotel = searchHotel; // Pass the search query back to the view
            }

            var reviews = await reviewsQuery.ToListAsync();
            return View(reviews);
        }
        [Authorize(Policy = "MustBeAnHotelManager")]
        public async Task<IActionResult> ListOfManagerReviews(string? searchHotel)
        {
            // Get the current user's RegisterId from the session
            int userId = HttpContext.Session.GetInt32("UserId") ?? UserHelper.GetUserIdFromClaims(User);

            // Ensure the user is logged in
            if (userId == 0)
            {
                return RedirectToAction("AccessDenied", "User");
            }

            // Fetch reviews for hotels managed by the logged-in manager
            var reviewsQuery = _dbContext.Reviews
                .Where(r => r.Hotel != null && r.Hotel.HotelManagerId == userId) // Filter by the manager's hotels
                .AsQueryable();

            // Filter by hotel name if search query is provided
            if (!string.IsNullOrEmpty(searchHotel))
            {
                reviewsQuery = reviewsQuery.Where(r => r.Hotel.HotelName != null && r.Hotel.HotelName.Contains(searchHotel));
                ViewBag.SearchHotel = searchHotel; // Pass the search query back to the view
            }

            var reviews = await reviewsQuery.ToListAsync();
            return View(reviews);
        }
        public async Task<IActionResult> ReviewDetails(int reviewId)

        {

            var review = await _dbContext.Reviews.FindAsync(reviewId);

            if (review == null)

            {

                return NotFound();

            }

            return View(review);

        }
        [Authorize(Policy = "MustBeAnMember")]
        public async Task<IActionResult> CreateReview(int hotelid)

        {

            int userId = HttpContext.Session.GetInt32("UserId") ?? UserHelper.GetUserIdFromClaims(User);

            var review = new Review()

            {

                HotelId = hotelid,

                TimeStamp = DateTime.Now,

                RegisterId = userId,

                HotelName = await _dbContext.Hotels.Where(h => h.HotelId == hotelid).Select(h => h.HotelName).FirstOrDefaultAsync()

            };

            return View(review);

        }
        [Authorize(Policy = "MustBeAnMember")]

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> CreateReview(Review review)

        {

            if (ModelState.IsValid)

            {

                try

                {

                    await _dbContext.Reviews.AddAsync(review);

                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction("ListOfUserReviews");

                }

                catch (Exception ex)

                {

                    ModelState.AddModelError("", $"Error: {ex.Message}");

                }

            }

            return View(review);

        }
        [Authorize(Policy = "MustBeAnMember")]
        public async Task<IActionResult> EditReview(int reviewId)

        {

            var review = await _dbContext.Reviews.FindAsync(reviewId);

            if (review == null)

            {

                return NotFound();

            }

            return View(review);

        }
        [Authorize(Policy = "MustBeAnMember")]

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> EditReview(Review review)

        {

            if (ModelState.IsValid)

            {

                try

                {

                    _dbContext.Reviews.Update(review);

                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction("ListOfUserReviews");

                }

                catch (Exception ex)

                {

                    ModelState.AddModelError("", $"Error: {ex.Message}");

                }

            }

            return View(review);

        }
        [Authorize(Policy = "MustBeAnHotelManager")]
        public async Task<IActionResult> DeleteReview(int reviewId)

        {

            if (!ModelState.IsValid)

            {

                return NotFound();

            }

            var review = await _dbContext.Reviews.FindAsync(reviewId);

            if (review == null)

            {

                return NotFound();

            }

            _dbContext.Reviews.Remove(review);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ListOfManagerReviews");

        }
        [Authorize(Policy = "MustBeAnAdmin")]
        public async Task<IActionResult> AdminDeleteReview(int reviewId)

        {

            if (!ModelState.IsValid)

            {

                return NotFound();

            }

            var review = await _dbContext.Reviews.FindAsync(reviewId);

            if (review == null)

            {

                return NotFound();

            }

            _dbContext.Reviews.Remove(review);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ListOfAdminReviews");

        }
        [Authorize(Policy = "MustBeAnMember")]
        public async Task<IActionResult> UserDeleteReview(int reviewId)

        {

            if (!ModelState.IsValid)

            {

                return NotFound();

            }

            var review = await _dbContext.Reviews.FindAsync(reviewId);

            if (review == null)

            {

                return NotFound();

            }

            _dbContext.Reviews.Remove(review);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("ListOfUserReviews");

        }

    }

}

