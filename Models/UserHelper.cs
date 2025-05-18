using System.Security.Claims;

namespace SmartHotelBooking.Helpers
{
    public static class UserHelper
    {
        public static int GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var userIdClaim = user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return 0; // Return 0 if the user ID is not found or invalid
        }
    }
}
