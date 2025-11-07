using System.ComponentModel.DataAnnotations;

namespace GuideMe.ViewModels
{
    public class ResendEmailConfirmationVm
    {
        public int Id { get; set; }
        [Required]
        public string UserNameOrEmail { get; set; } = string.Empty;
    }
}
