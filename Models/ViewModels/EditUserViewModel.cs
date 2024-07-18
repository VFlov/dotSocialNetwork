using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Net;
using System.Reflection;

namespace dotSocialNetwork.Models.ViewModels
{
    public class EditUserViewModel
    {
        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required]
        public string FirstName { get; set; }
        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required]
        public string LastName { get; set; }

        [Display(Name = "День рождения"), DataType(DataType.Date), DisplayFormat, Required]
        public DateTime BirthDate { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Display(Name = "Отчество", Prompt = "Введите отчество")]
        public string MiddleName { get; set; }

        [DataType(DataType.ImageUrl), Display(Name = "Фото", Prompt = "Укажите ссылку на картинку")]
        public string ImageUrl { get; set; }

        [DataType(DataType.Text), Display(Name = "Статус", Prompt = "Укажите статус")]
        public string Status { get; set; }

        [DataType(DataType.Text), Display(Name = "О себе", Prompt = "Введите данные о себе")]
        public string About { get; set; }
        
        public EditUserViewModel(User user) 
        {
            FirstName = user.FirstName;
            LastName = user.LastName;
            MiddleName = user.MiddleName;
            Password = user.Password;
            Email = user.Email;
            ImageUrl = user.Image;
            Status = user.Status;
            About = user.About;
        }
        public EditUserViewModel() 
        {
            FirstName = "";
            LastName = "";
            MiddleName = "";
            BirthDate = new DateTime();
            Email = "";
            ImageUrl = "";
            Status = "";
            About = "";
            Password = "";
        }
        
    }
}
