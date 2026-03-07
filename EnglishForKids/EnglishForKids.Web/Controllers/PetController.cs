using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnglishForKids.Infrastructure.Data;
using EnglishForKids.Data.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EnglishForKids.Web.Controllers
{
    public class PetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Pet/Index
        public async Task<IActionResult> Index()
        {
            var childId = HttpContext.Session.GetInt32("ChildId");
            if (childId == null)
            {
                return RedirectToAction("ChildLogin", "Account");
            }

            var pet = await _context.VirtualPets
                .FirstOrDefaultAsync(p => p.ChildProfileId == childId);

            if (pet == null)
            {
                // Создаем питомца, если его нет
                pet = new VirtualPet
                {
                    ChildProfileId = childId.Value,
                    PetType = GetRandomPetType(),
                    PetName = "Дружок",
                    Level = 1,
                    Experience = 0,
                    Happiness = 50,
                    Food = 50,
                    Energy = 100,
                    Accessory = "",
                    LastFed = DateTime.Now,
                    LastPlayed = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
                _context.VirtualPets.Add(pet);
                await _context.SaveChangesAsync();
            }

            return View(pet);
        }

        // POST: /Pet/Feed
        [HttpPost]
        public async Task<IActionResult> Feed()
        {
            var childId = HttpContext.Session.GetInt32("ChildId");
            if (childId == null)
            {
                return Json(new { success = false, message = "Необходимо войти в систему" });
            }

            var pet = await _context.VirtualPets
                .FirstOrDefaultAsync(p => p.ChildProfileId == childId);

            if (pet != null)
            {
                pet.Food = Math.Min(100, pet.Food + 20);
                pet.Happiness = Math.Min(100, pet.Happiness + 5);
                pet.LastFed = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    food = pet.Food,
                    happiness = pet.Happiness
                });
            }

            return Json(new { success = false, message = "Питомец не найден" });
        }

        // POST: /Pet/Play
        [HttpPost]
        public async Task<IActionResult> Play()
        {
            var childId = HttpContext.Session.GetInt32("ChildId");
            if (childId == null)
            {
                return Json(new { success = false, message = "Необходимо войти в систему" });
            }

            var pet = await _context.VirtualPets
                .FirstOrDefaultAsync(p => p.ChildProfileId == childId);

            if (pet != null)
            {
                pet.Happiness = Math.Min(100, pet.Happiness + 15);
                pet.Energy = Math.Max(0, pet.Energy - 10);
                pet.LastPlayed = DateTime.Now;

                // Добавляем опыт
                pet.Experience += 5;

                // Проверка на повышение уровня
                if (pet.Experience >= pet.Level * 100)
                {
                    pet.Level++;
                    pet.Experience = 0;
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    happiness = pet.Happiness,
                    energy = pet.Energy,
                    level = pet.Level,
                    experience = pet.Experience
                });
            }

            return Json(new { success = false, message = "Питомец не найден" });
        }

        private string GetRandomPetType()
        {
            string[] types = { "Дракончик", "Енот", "Совенок", "Панда", "Лисенок" };
            Random rand = new Random();
            return types[rand.Next(types.Length)];
        }
    }
}