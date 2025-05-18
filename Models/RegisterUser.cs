using System.ComponentModel.DataAnnotations;

namespace SmartHotelBooking.Models
{
    public class RegisterUser
    {
        [Key]
        public int RegisterId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Email is required.")]
        [Compare("Email", ErrorMessage = "Email and Confirm Email should match.")]
        public string ConfirmEmailId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password should match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        public string PhoneNumber { get; set; } = string.Empty;

        public int BookedCount { get; set; } = 0; // Number of rooms booked
        public string? Address { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = string.Empty;

        public bool IsLoyalCustomer { get; set; } = false;

        public int RedemptionPoints { get; set; } = 0; // Points for all customers
       
    }
}

