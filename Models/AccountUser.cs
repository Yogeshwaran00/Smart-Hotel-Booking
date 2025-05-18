using Microsoft.AspNetCore.Identity;

namespace SmartHotelBooking.Models
{
    public class AccountUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? Address { get; set; }
    }
}