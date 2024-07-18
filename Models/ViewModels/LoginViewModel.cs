using System.ComponentModel.DataAnnotations;

namespace dotSocialNetwork.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required, EmailAddress, Display(Name = "Email", Prompt = "Введите Email")]
        public string Email { get; set; }
        [Required, DataType(DataType.Password), Display(Name = "Пароль", Prompt = "Введите пароль")]
        public string Password { get; set; }
    }
}
