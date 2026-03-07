using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnglishForKids.Data.Entities.Identity;
using EnglishForKids.Data.Entities;
using EnglishForKids.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;  
using System;
using EnglishForKids.Web.ViewModels;

namespace EnglishForKids.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string email, string password, string fullName)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View();
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(email, password, true, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Неверный email или пароль");
            }
            return View();
        }

        // GET: /Account/Test
        public IActionResult Test()
        {
            return Content("AccountController работает!");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/ParentDashboard
        public async Task<IActionResult> ParentDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var children = await _context.ChildProfiles
                .Where(c => c.ParentId == user.Id)
                .Include(c => c.Progresses)
                    .ThenInclude(p => p.Topic)
                .Include(c => c.VirtualPet)
                .ToListAsync();

            return View(children);
        }

        // GET: /Account/AddChild
        public IActionResult AddChild()
        {
            return View();
        }

        // POST: /Account/AddChild
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChild(string name, int age, int grade, string avatar)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Проверка - не больше 3 детей
            var childrenCount = await _context.ChildProfiles
                .CountAsync(c => c.ParentId == user.Id);

            if (childrenCount >= 3)
            {
                ModelState.AddModelError("", "Вы можете добавить не более 3 детей");
                return View();
            }

            // Генерируем уникальный PIN-код для ребенка (4 цифры)
            string pinCode;
            bool pinExists;

            do
            {
                var random = new Random();
                pinCode = random.Next(1000, 9999).ToString();

                // Проверяем, что PIN уникален в пределах этого родителя
                pinExists = await _context.ChildProfiles
                    .AnyAsync(c => c.PinCode == pinCode && c.ParentId == user.Id);

            } while (pinExists); // Генерируем новый, если такой PIN уже есть у этого родителя

            var child = new ChildProfile
            {
                Name = name,
                Age = age,
                Grade = grade,
                AvatarUrl = avatar ?? "/images/avatars/default.png",
                PinCode = pinCode,
                ParentId = user.Id
            };

            _context.ChildProfiles.Add(child);
            await _context.SaveChangesAsync();

            // Создаем виртуального питомца для ребенка
            var pet = new VirtualPet
            {
                ChildProfileId = child.Id,
                PetType = "Дракончик",
                Level = 1,
                Happiness = 50,
                Food = 50,
                Accessory = ""
            };
            _context.VirtualPets.Add(pet);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Ребенок успешно добавлен! PIN-код: {pinCode}";
            return RedirectToAction("ParentDashboard");
        }

        // GET: /Account/ChildLogin
        public IActionResult ChildLogin()
        {
            return View();
        }

        // POST: /Account/ChildLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChildLogin(ChildLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Ищем родителя по email
            var parent = await _userManager.FindByEmailAsync(model.ParentEmail);
            if (parent == null)
            {
                ModelState.AddModelError("", "Родитель с таким email не найден");
                return View(model);
            }

            // Ищем ребенка с таким PIN-кодом у этого родителя
            var child = await _context.ChildProfiles
                .Include(c => c.VirtualPet)
                .FirstOrDefaultAsync(c => c.PinCode == model.PinCode && c.ParentId == parent.Id);

            if (child != null)
            {
                HttpContext.Session.SetInt32("ChildId", child.Id);
                HttpContext.Session.SetString("ChildName", child.Name);
                HttpContext.Session.SetString("ParentEmail", parent.Email); // Сохраняем email родителя в сессии

                return RedirectToAction("ChildDashboard");
            }

            ModelState.AddModelError("", "Неверный PIN-код для этого родителя");
            return View(model);
        }

        // GET: /Account/ChildDashboard
        [HttpGet]
        public async Task<IActionResult> ChildDashboard()
        {
            var childId = HttpContext.Session.GetInt32("ChildId");
            if (childId == null)
            {
                return RedirectToAction("ChildLogin");
            }

            var child = await _context.ChildProfiles
                .Include(c => c.Progresses)
                    .ThenInclude(p => p.Topic)
                .Include(c => c.VirtualPet)
                .Include(c => c.UserAnswers)  
                .FirstOrDefaultAsync(c => c.Id == childId);

            if (child == null)
            {
                return RedirectToAction("ChildLogin");
            }

            return View(child);
        }

        // GET: /Account/ChildLogout
        [HttpGet("ChildLogout")]
        public IActionResult ChildLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}