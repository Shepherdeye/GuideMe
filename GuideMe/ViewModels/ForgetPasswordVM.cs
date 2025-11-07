using System.ComponentModel.DataAnnotations;

namespace GuideMe.Models
{
    public class ForgetPasswordVM
    {
        public int Id { get; set; }
        [Required]
        public string UserNameOrEmail { get; set; }=string.Empty;
      
    }
}
