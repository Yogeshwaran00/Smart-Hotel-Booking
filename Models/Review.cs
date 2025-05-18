using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHotelBooking.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int RegisterId { get; set; } // Foreign key to User

        public int HotelId { get; set; } // Foreign key to Hotel
        public double Rating { get; set; } // Rating out of 5

        public string? Comment { get; set; }

        public string? HotelName { get; set; }

        public DateTime TimeStamp { get; set; } // Date and time of the review
        public Hotel? Hotel { get; set; } // Navigation property to Hotel
    }
}
