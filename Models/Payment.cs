namespace SmartHotelBooking.Models

{

    public class Payment

    {

        public int PaymentId { get; set; }

        public int UserId { get; set; }

        public int BookingId { get; set; } // Foreign key to Booking

        public double Amount { get; set; }

        public string? PaymentMethod { get; set; } // e.g., Credit Card, PayPal

        public string? Status { get; set; }

    }

}

