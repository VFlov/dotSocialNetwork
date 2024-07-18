using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using dotSocialNetwork.Data;
using dotSocialNetwork.Models;
using dotSocialNetwork.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore.Infrastructure;
using dotSocialNetwork.Repository;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NuGet.Versioning;
using System.Net;
using System.Reflection;
using System.Drawing;
using System.Security.Policy;

namespace dotSocialNetwork.Controllers
{
    public class UsersController : Controller
    {
        private readonly dotSocialNetworkContext _context;
        private IUnitOfWork _unitOfWork;

        public UsersController(dotSocialNetworkContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        // GET: Users
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Email, Password")] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User localUser = await _context.User.FirstOrDefaultAsync(m => m.Email == model.Email && m.Password == model.Password);
                if (localUser != null)
                {
                    //Problem("", "Пользователь найден");
                    var claims = new List<Claim> { new Claim(ClaimTypes.Name, model.Email) };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction("MyPage");

                }
                else
                {
                    //Problem("","Неправильный логин и (или) пароль");
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User()
                {
                    FirstName = model.FirstName,
                    MiddleName = "",
                    LastName = model.LastName,
                    Email = model.Email,
                    BirthDate = model.BirthDate,
                    Password = model.PasswordConfirm,
                    Image = "https://thispersondoesnotexist.com",
                    Status = "",
                    About = ""
                };
                var createUser = await _context.AddAsync<User>(user);
                if (createUser != null)
                {
                    _context.SaveChanges();
                    ModelState.AddModelError("", "Пользователь создан");
                }
                else
                {
                    ModelState.AddModelError("", "Пользователь НЕ создан");
                }
            }
            return View("RegisterPart2", model);
        }
        public IActionResult RegisterPart2(RegisterViewModel model)
        {
            return View("RegisterPart2", model);
        }
        [Authorize]
        public async Task<IActionResult> MyPage()
        {
            var localUser = FindLocalUser();
            var model = new UserViewModel(localUser);
            model.Friends = await GetAllFriends(model.User);
            return View("UserPage", model);
        }
        //NOT ASYNC. Need to fix
        private async Task<List<User>> GetAllFriends(User user)
        {
            var repository = _unitOfWork.GetRepository<Friend>() as FriendsRepository;
            return repository.GetFriendsByUser(user);
        }
        private async Task<List<User>> GetAllFriends()
        {
            var repository = _unitOfWork.GetRepository<Friend>() as FriendsRepository;
            return repository.GetFriendsByUser(FindLocalUser());
        }

        public IActionResult Edit()
        {
            var model = new EditUserViewModel(FindLocalUser());
            return View("Edit", model);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Update(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = FindLocalUser();
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.MiddleName = model.MiddleName;
                user.Email = model.Email;
                user.Password = model.Password;
                user.BirthDate = model.BirthDate;
                user.About = model.About;
                user.Status = model.Status;
                user.Image = model.ImageUrl;
                _context.SaveChanges();
                return RedirectToAction("MyPage");
            }
            else
            {
                ModelState.AddModelError("", "Некорректные данные");
                return View("Edit", model);
            }
        }
        [HttpPost, Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> UserList(string search)
        {
            var model = await CreateSearch(search);
            return View("UserList", model);
        }
        public async Task<SearchViewModel> CreateSearch(string search)
        {

            var listOfUsers = _context.User.AsEnumerable().Where(x => x.FullName.ToLower().Contains(search.ToLower())).ToList();

            /* Вывести всех пользователей без друзей
            var userFriends = await GetAllFriends();
            foreach (var user in listOfUsers)
            {
                if (userFriends.Contains(user))
                    userFriends.Remove(user);
            }*/
            return new SearchViewModel() { UserList = listOfUsers };
        }
        [HttpPost]
        public async Task<IActionResult> AddFriend(string id)
        {
            var friendModel = await _context.User.FirstAsync(u => u.Id == int.Parse(id));
            if (friendModel != null)
            {
                var repository = _unitOfWork.GetRepository<Friend>() as FriendsRepository;
                repository.AddFriend(FindLocalUser(), friendModel);
            }
            return RedirectToAction("MyPage");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteFriend(string id)
        {
            var friendModel = await _context.User.FirstAsync(u => u.Id == int.Parse(id));
            if (friendModel != null)
            {
                var repository = _unitOfWork.GetRepository<Friend>() as FriendsRepository;
                repository.DeleteFriend(FindLocalUser(), friendModel);
            }
            return RedirectToAction("MyPage");
        }
        [HttpPost]
        public async Task<IActionResult> Chat(string id)
        {
            var model = GenerateChat(int.Parse(id));
            return View(model);
        }
        private ChatViewModel GenerateChat(int id)
        {
            var localUser = FindLocalUser();
            var interlocutor = _context.User.FirstOrDefault(i => i.Id == id);
            Console.WriteLine("GG");
            var repository = _unitOfWork.GetRepository<Message>() as MessageRepository;
            var messages = repository.GetMessages(localUser, interlocutor);
            return new ChatViewModel()
            {
                User = localUser,
                Interlocutor = interlocutor,
                History = messages.OrderBy(x => x.Id).ToList()
            };
        }
        [HttpGet]
        public async Task<IActionResult> Chat()
        {
            var id = Request.Query["id"];
            var model = GenerateChat(id);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> NewMessage(string id, ChatViewModel chat)
        {
            var localUser = FindLocalUser();
            var interlocutor = await _context.User.FirstOrDefaultAsync(i => i.Id == int.Parse(id));
            var repository = _unitOfWork.GetRepository<Message>() as MessageRepository;
            var item = new Message()
            {
                SenderId = localUser.Id.ToString(),
                RecipientId = interlocutor.Id.ToString(),
                Text = chat.NewMessage.Text
            };
            repository.Create(item);
            var model = GenerateChat(id);
            return View("Chat", model);
        }

        public User FindLocalUser()
        {
            return _context.User.FirstOrDefault<User>(u => u.Email == HttpContext.User.Identity.Name)!;
        }
        

        /*
        private Image GetImageFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return GetDefaultImage();
            }
            else
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        byte[] imageData = client.DownloadData(url);
                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            return Image.FromStream(ms);
                        }
                    }
                }
                catch
                {
                    return GetDefaultImage();
                }
            }
        }
        private Image GetDefaultImage()
        {
            using (WebClient client = new WebClient())
            {
                byte[] imageData = client.DownloadData("https://sun9-56.userapi.com/s/v1/if1/oe9InPdFVkopZS4yhFazjDt2wtvgTH2zV1IAhY49RJaJFcf-frOrQ4in8fI3gg8UzJfBQIPm.jpg?size=289x289&quality=96&crop=62,53,289,289&ava=1");
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    return Image.FromStream(ms);
                }
            }
        }
        */


        //==============================================================================
        // GET: Users/Details/5
        /*
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,MiddleName,BirthDate,Password,Email")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,MiddleName,BirthDate,Password,Email")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }
        */
    }
}
