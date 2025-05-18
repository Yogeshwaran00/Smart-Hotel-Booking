using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartHotelBooking.Models;

namespace SmartHotelBooking.Models

{

    public class Hotel

    {

        [Key]

        public int HotelId { get; set; }

        [Required]

        public string? HotelName { get; set; }

        [Required]

        public string? HotelLocation { get; set; }

        [Required]

        public int HotelManagerId { get; set; }

        [Required]

        public string? HotelAmenities { get; set; }

        [Required]

        public double HotelRating { get; set; }

        [Required]

        public string? HotelDescription { get; set; }

        [Required]

        public double HotelPricePerNight { get; set; }

        [Required]

        public string? HotelImageUrl { get; set; }

        public ICollection<Room>? Rooms { get; set; }

       

    }

}
