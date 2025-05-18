using Microsoft.AspNetCore.Mvc;
using SmartHotelBooking.Models;
using Stripe.Checkout;

namespace SmartHotelBooking.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult MakePayment()
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData=new SessionLineItemPriceDataOptions
                        {
                            Currency="usd",
                            ProductData=new SessionLineItemPriceDataProductDataOptions
                            {
                                Name="Room Booking",
                            },
                            UnitAmount=(long)(5*100),
                        },
                        Quantity=1,
                    }
                },
                Mode = "payment",
                SuccessUrl = Url.Action("AccountCreated", "User", null, Request.Scheme),//action-controller
                CancelUrl = Url.Action("PaymentDeined", "Payment", null, Request.Scheme),

            };
            try
            {
                var service = new SessionService();
                Session session = service.Create(options);
                var paymentUrl = session.Url;
                // Redirect to the payment URL
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View();
            }   
            return View();  
        }
    }
}
