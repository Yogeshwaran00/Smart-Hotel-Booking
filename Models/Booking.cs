using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHotelBooking.Models

{

    public class Booking

    {
        [Key]
        public int BookingId { get; set; }
        public int RegisterId { get; set; } // Foreign key to User

        public int RoomId { get; set; } // Foreign key to Room
        [Required]
        public DateTime CheckInDate { get; set; }
        [Required]
        public DateTime CheckOutDate { get; set; }

        public double? TotalPrice { get; set; }
        [Required]
        public string? PaymentMethod { get; set; } // e.g., Credit Card, PayPal

        public string? Status { get; set; } // e.g., Confirmed, Cancelled

        public int PointsEarned { get; set; } = 0;
        public Room? Room { get; set; }// Navigation property to Room
        [ForeignKey("RegisterId")]
        public RegisterUser? RegisterUser { get; set; } // Navigation property to User
    }

}
