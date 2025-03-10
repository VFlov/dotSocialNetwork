using Microsoft.AspNetCore.Mvc;
using dotSocialNetwork.Server.Models;

namespace dotSocialNetwork.Server.Controllers
{
    public class ValheimController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("api/news")]
        public IActionResult GetNews()
        {
            var news = new List<NewsItem>
        {
            new NewsItem { Id = 1, Title = "Запуск сервера!", Content = "Наш Valheim сервер официально запущен!", Date = DateTime.Now.AddDays(-2) },
            new NewsItem { Id = 2, Title = "Обновление 1.1", Content = "Добавлены новые биомы и исправлены баги", Date = DateTime.Now }

        };
            return Ok(System.Text.Json.JsonSerializer.Serialize<List<NewsItem>>(news));
        }
    }
}
