using System.ComponentModel.DataAnnotations;

namespace GuideMe.ViewModels
{
    public class VisitorDetailsVm
    {
        public int Id { get; set; }
        [Required,MinLength(14),MaxLength(14)]
        public string Passport { get; set; }= string.Empty;
        public VisitorStatus visitorStatus { get; set; }
    }
}
