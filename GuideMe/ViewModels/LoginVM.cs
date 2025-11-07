using System.ComponentModel.DataAnnotations;

namespace GuideMe.ViewModels
{
    public class LoginVM
    {
        public int Id { get; set; }

        [Required]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required,DataType(DataType.Password)]
        public string Password { get; set; }=string.Empty;

        [Required]
        public  bool RememberMe { get; set; }
    }
}
