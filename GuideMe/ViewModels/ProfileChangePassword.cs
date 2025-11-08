using System.ComponentModel.DataAnnotations;

namespace GuideMe.ViewModels
{
    public class ProfileChangePassword
    {

        public string ApplicationUserId { get; set; }=string.Empty;

        [Required,DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required,DataType(DataType.Password)]
        public string NewPassword { get; set; }=string.Empty;

        [Required,DataType(DataType.Password),Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; }=string.Empty;
    }
}
