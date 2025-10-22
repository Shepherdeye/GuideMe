using System.ComponentModel.DataAnnotations;

namespace GuideMe.Models
{
    public enum OfferStatus
    {
        Active,
        Taken,
        Expired
    }


    public class Offer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GuideId { get; set; }
        public Guide Guide { get; set; }

        [Required]
        public int TripId { get; set; }

        public Trip Trip { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public OfferStatus Availability { get; set; }

        [Required]
        [Range(10,double.MaxValue)]
        public decimal OfferedPrice { get; set; }

        [Required]
        public DateTime OfferStartDate { get; set; }


        [Required]
        public DateTime OfferEndDate { get; set; }

        

    }
}
