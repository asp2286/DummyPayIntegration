using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace DummyPayIntegration.Models
{
    public class PaymentCreationRequestData
    {
        [Required]
        public string OrderId { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        public string Currency { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        [StringLength(19, MinimumLength = 12)]
        public string CardNumber { get; set; }

        [Required]
        public string CardHolder { get; set; }

        [Required]
        public string CardExpiryDate { get; set; }

        [Required]
        [StringLength(3, ErrorMessage = "the CVV must contain 3 digits", MinimumLength = 3)]
        public string Cvv { get; set; }
    }
}
