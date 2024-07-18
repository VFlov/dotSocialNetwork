using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace dotSocialNetwork.Models
{
    public class User 
    {
        public int Id { get; set; }
        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required]
        public string FirstName { get; set; } = "";
        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required]
        public string LastName { get; set; } = "";

        [Display(Name = "День рождения"), DataType(DataType.Date),DisplayFormat, Required]
        public DateTime BirthDate { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";
        [Required]
        public string Email { get; set; } = "";


        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Display(Name ="Отчество", Prompt = "Введите отчество")]
        public string MiddleName { get; set; } = "";

        [DataType(DataType.ImageUrl), Display(Name = "Фото", Prompt = "Укажите ссылку на картинку")]
        public string Image { get; set; } = "";

        [DataType(DataType.Text), Display(Name = "Статус", Prompt = "Укажите статус")]
        public string Status { get; set; } = "";

        [DataType(DataType.Text), Display(Name = "О себе", Prompt = "Введите данные о себе")]
        public string About { get; set; } = "";

        public string FullName => FirstName + " " + MiddleName + " " + LastName;
    }
}
