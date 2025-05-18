using System.ComponentModel.DataAnnotations;

namespace SmartHotelBooking.Models
{
    public class Room
    {
        [Required]
        public int RoomId { get; set; }
        [Required]
        public int HotelId { get; set; } // Foreign key to Hotel
        [Required]
        public string? RoomType { get; set; }
        [Required]
        public string? Features { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public int Capacity { get; set; }
        [Required]
        public bool IsAvailable { get; set; }
        public Booking? Booking { get; set; } // Navigation property to Booking
        public Hotel? Hotel { get; set; } // Navigation property to Hotel
    }
}
