using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnglishForKids.Infrastructure.Data;
using EnglishForKids.Data.Entities;
using EnglishForKids.Data.Enums;
using EnglishForKids.Web.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using EnglishForKids.Data.Entities.Identity;

namespace EnglishForKids.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalChildren = await _context.ChildProfiles.CountAsync(),
                TotalTopics = await _context.Topics.CountAsync(),
                TotalQuestions = await _context.Questions.CountAsync(),
                TotalAnswers = await _context.UserAnswers.CountAsync(),
                RecentTopics = await _context.Topics
                    .OrderByDescending(t => t.Id)
                    .Take(5)
                    .ToListAsync(),
                RecentChildren = await _context.ChildProfiles
                    .Include(c => c.Parent)
                    .OrderByDescending(c => c.Id)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // ========== УПРАВЛЕНИЕ ТЕМАМИ ==========

        // GET: /Admin/Topics
        public async Task<IActionResult> Topics()
        {
            var topics = await _context.Topics
                .Include(t => t.Category)
                .OrderBy(t => t.CategoryId)
                .ThenBy(t => t.Order)
                .ToListAsync();

            return View(topics);
        }

        // GET: /Admin/CreateTopic
        public async Task<IActionResult> CreateTopic()
        {
            var viewModel = new TopicAdminViewModel
            {
                Categories = await _context.Categories.ToListAsync(),
                Order = 1
            };
            return View(viewModel);
        }

        // POST: /Admin/CreateTopic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTopic(TopicAdminViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var topic = new Topic
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    CategoryId = viewModel.CategoryId,
                    Order = viewModel.Order,
                    IconUrl = viewModel.IconUrl ?? "/images/topics/default.png"
                };

                _context.Topics.Add(topic);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Тема успешно создана!";
                return RedirectToAction("Topics");
            }

            viewModel.Categories = await _context.Categories.ToListAsync();
            return View(viewModel);
        }

        // GET: /Admin/EditTopic/5
        public async Task<IActionResult> EditTopic(int id)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic == null)
            {
                return NotFound();
            }

            var viewModel = new TopicAdminViewModel
            {
                Id = topic.Id,
                Title = topic.Title,
                Description = topic.Description,
                CategoryId = topic.CategoryId,
                Order = topic.Order,
                IconUrl = topic.IconUrl,
                Categories = await _context.Categories.ToListAsync()
            };

            return View(viewModel);
        }

        // POST: /Admin/EditTopic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTopic(TopicAdminViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var topic = await _context.Topics.FindAsync(viewModel.Id);
                if (topic == null)
                {
                    return NotFound();
                }

                topic.Title = viewModel.Title;
                topic.Description = viewModel.Description;
                topic.CategoryId = viewModel.CategoryId;
                topic.Order = viewModel.Order;
                topic.IconUrl = viewModel.IconUrl;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Тема успешно обновлена!";
                return RedirectToAction("Topics");
            }

            viewModel.Categories = await _context.Categories.ToListAsync();
            return View(viewModel);
        }

        // POST: /Admin/DeleteTopic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            var topic = await _context.Topics
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (topic == null)
            {
                return NotFound();
            }

            if (topic.Questions.Any())
            {
                TempData["Error"] = "Нельзя удалить тему, в которой есть вопросы!";
                return RedirectToAction("Topics");
            }

            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Тема успешно удалена!";
            return RedirectToAction("Topics");
        }

        // ========== УПРАВЛЕНИЕ ВОПРОСАМИ ==========

        // GET: /Admin/Questions
        public async Task<IActionResult> Questions(int? topicId)
        {
            var query = _context.Questions
                .Include(q => q.Topic)
                .Include(q => q.Options)
                .AsQueryable();

            if (topicId.HasValue)
            {
                query = query.Where(q => q.TopicId == topicId);
            }

            var questions = await query
                .OrderBy(q => q.TopicId)
                .ThenBy(q => q.Difficulty)
                .ToListAsync();

            ViewBag.Topics = await _context.Topics.ToListAsync();
            ViewBag.SelectedTopicId = topicId;

            return View(questions);
        }

        // GET: /Admin/CreateQuestion
        public async Task<IActionResult> CreateQuestion()
        {
            var viewModel = new QuestionAdminViewModel
            {
                Topics = await _context.Topics.ToListAsync(),
                Difficulty = 1
            };
            return View(viewModel);
        }

        // POST: /Admin/CreateQuestion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion(QuestionAdminViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var question = new Question
                {
                    TopicId = viewModel.TopicId,
                    QuestionType = (QuestionType)viewModel.QuestionType,
                    Text = viewModel.Text,
                    ImageUrl = viewModel.ImageUrl,
                    AudioUrl = viewModel.AudioUrl,
                    Difficulty = (DifficultyLevel)viewModel.Difficulty,
                    IsTrueStatement = viewModel.IsTrueStatement
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                // Если это Multiple Choice или Translation, перенаправляем на добавление вариантов
                if (viewModel.QuestionType == 2 || viewModel.QuestionType == 4)
                {
                    return RedirectToAction("CreateOption", new { questionId = question.Id });
                }

                TempData["Success"] = "Вопрос успешно создан!";
                return RedirectToAction("Questions");
            }

            viewModel.Topics = await _context.Topics.ToListAsync();
            return View(viewModel);
        }

        // GET: /Admin/CreateOption/5
        public async Task<IActionResult> CreateOption(int questionId)
        {
            var question = await _context.Questions
                .Include(q => q.Topic)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
            {
                return NotFound();
            }

            var viewModel = new OptionAdminViewModel
            {
                QuestionId = questionId,
                Order = 1
            };

            ViewBag.Question = question;
            return View(viewModel);
        }

        // POST: /Admin/CreateOption
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOption(OptionAdminViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var option = new QuestionOption
                {
                    QuestionId = viewModel.QuestionId,
                    Text = viewModel.Text,
                    IsCorrect = viewModel.IsCorrect,
                    Order = viewModel.Order
                };

                _context.QuestionOptions.Add(option);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Вариант ответа добавлен!";
                return RedirectToAction("EditQuestion", new { id = viewModel.QuestionId });
            }

            return View(viewModel);
        }

        // GET: /Admin/EditQuestion/5
        public async Task<IActionResult> EditQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .Include(q => q.Topic)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // ========== УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ ==========

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // POST: /Admin/MakeAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Создаем роль Admin, если её нет
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            await _userManager.AddToRoleAsync(user, "Admin");
            TempData["Success"] = $"Пользователь {user.Email} теперь администратор!";
            return RedirectToAction("Users");
        }

        // ========== СТАТИСТИКА ==========

        // GET: /Admin/Statistics
        public async Task<IActionResult> Statistics()
        {
            var stats = new
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalChildren = await _context.ChildProfiles.CountAsync(),
                TotalTopics = await _context.Topics.CountAsync(),
                TotalQuestions = await _context.Questions.CountAsync(),
                TotalAnswers = await _context.UserAnswers.CountAsync(),

                AnswersByDay = await _context.UserAnswers
                    .GroupBy(a => a.AttemptDate.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Date)
                    .Take(7)
                    .ToListAsync(),

                PopularTopics = await _context.Questions
                    .GroupBy(q => q.TopicId)
                    .Select(g => new { TopicId = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .Take(5)
                    .ToListAsync()
            };

            return View(stats);
        }

        // GET: /Admin/Rules
        public async Task<IActionResult> Rules(int? topicId)
        {
            var query = _context.Rules
                .Include(r => r.Topic)
                .AsQueryable();

            if (topicId.HasValue && topicId.Value > 0)
            {
                query = query.Where(r => r.TopicId == topicId.Value);
            }

            var rules = await query
                .OrderBy(r => r.TopicId)
                .ThenBy(r => r.Id)
                .ToListAsync();

            ViewBag.Topics = await _context.Topics.ToListAsync();
            ViewBag.SelectedTopicId = topicId;

            return View(rules);
        }

        // GET: /Admin/CreateRule
        public async Task<IActionResult> CreateRule()
        {
            var viewModel = new RuleAdminViewModel
            {
                Topics = await _context.Topics.ToListAsync()
            };
            return View(viewModel);
        }

        // POST: /Admin/CreateRule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRule(RuleAdminViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var rule = new Rule
                {
                    TopicId = viewModel.TopicId,
                    Title = viewModel.Title,
                    TheoryText = viewModel.TheoryText,
                    ExampleText = viewModel.ExampleText,
                    VideoUrl = viewModel.VideoUrl
                };

                _context.Rules.Add(rule);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Правило успешно создано!";
                return RedirectToAction("Rules");
            }

            viewModel.Topics = await _context.Topics.ToListAsync();
            return View(viewModel);
        }

        // GET: /Admin/EditRule/5
        public async Task<IActionResult> EditRule(int id)
        {
            var rule = await _context.Rules.FindAsync(id);
            if (rule == null)
            {
                return NotFound();
            }

            var viewModel = new RuleAdminViewModel
            {
                Id = rule.Id,
                TopicId = rule.TopicId,
                Title = rule.Title,
                TheoryText = rule.TheoryText,
                ExampleText = rule.ExampleText,
                VideoUrl = rule.VideoUrl,
                Topics = await _context.Topics.ToListAsync()
            };

            return View(viewModel);
        }

        // POST: /Admin/EditRule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRule(RuleAdminViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var rule = await _context.Rules.FindAsync(viewModel.Id);
                if (rule == null)
                {
                    return NotFound();
                }

                rule.TopicId = viewModel.TopicId;
                rule.Title = viewModel.Title;
                rule.TheoryText = viewModel.TheoryText;
                rule.ExampleText = viewModel.ExampleText;
                rule.VideoUrl = viewModel.VideoUrl;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Правило успешно обновлено!";
                return RedirectToAction("Rules");
            }

            viewModel.Topics = await _context.Topics.ToListAsync();
            return View(viewModel);
        }

        // POST: /Admin/DeleteRule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRule(int id)
        {
            var rule = await _context.Rules
                .Include(r => r.Questions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rule == null)
            {
                return NotFound();
            }

            // Проверяем, есть ли вопросы, привязанные к этому правилу
            if (rule.Questions != null && rule.Questions.Any())
            {
                TempData["Error"] = "Нельзя удалить правило, к которому привязаны вопросы!";
                return RedirectToAction("Rules");
            }

            _context.Rules.Remove(rule);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Правило успешно удалено!";
            return RedirectToAction("Rules");
        }
    }
}