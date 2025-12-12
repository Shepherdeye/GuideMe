using System.ComponentModel.DataAnnotations;

namespace GuideMe.ViewModels
{
    public class EditOfferVm
    {

        public int Id { get; set; }

        public int TripId { get; set; }
        [Required]
        public string Message { get; set; }

        [Required]
        [Range(10, double.MaxValue)]
        public decimal OfferedPrice { get; set; }

        [Required]
        public DateTime OfferStartDate { get; set; }


        [Required]
        public DateTime OfferEndDate { get; set; }
    }
}
