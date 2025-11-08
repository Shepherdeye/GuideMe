using System.ComponentModel.DataAnnotations;

namespace GuideMe.ViewModels
{
    public class ChangePasswordVM
    {
        public int Id { get; set; }

        [Required,DataType(DataType.Password)] 
        public string  Password { get; set; } =string.Empty;

        [Required, DataType(DataType.Password) ,Compare(nameof(Password))]

        public string  ConfirmPassword { get; set; } = string.Empty;

        public string  ApplicationUserId { get; set; }

    }
}
