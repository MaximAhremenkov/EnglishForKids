using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnglishForKids.Infrastructure.Data;
using EnglishForKids.Data.Entities;
using EnglishForKids.Data.Enums;
using EnglishForKids.Web.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace EnglishForKids.Web.Controllers
{
    public class GameController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GameController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Game/Index
        public async Task<IActionResult> Index()
        {
            var topics = await _context.Topics
                .Include(t => t.Category)
                .Where(t => t.Questions.Any())
                .ToListAsync();
            return View(topics);
        }

        // GET: /Game/SelectMode/5
        public async Task<IActionResult> SelectMode(int topicId)
        {
            var topic = await _context.Topics.FindAsync(topicId);
            if (topic == null)
            {
                return NotFound();
            }

            return View(topic);
        }

        // GET: /Game/Play/5?mode=1
        public async Task<IActionResult> Play(int topicId, int mode)
        {
            var childId = HttpContext.Session.GetInt32("ChildId");
            if (childId == null)
            {
                return RedirectToAction("ChildLogin", "Account");
            }

            // Получаем все вопросы для темы и режима
            IQueryable<Question> query = _context.Questions
                .Where(q => q.TopicId == topicId && (int)q.QuestionType == mode);

            // Загружаем связанные данные в зависимости от режима
            switch (mode)
            {
                case 2: // Multiple Choice
                case 4: // Translation
                    query = query.Include(q => q.Options.OrderBy(o => o.Order));
                    Console.WriteLine($"Play: Режим {mode} - загружаем Options");
                    break;
                case 5: // Sentence Builder
                    query = query.Include(q => q.SentenceParts.OrderBy(s => s.WordOrder));
                    Console.WriteLine($"Play: Режим {mode} - загружаем SentenceParts");
                    break;
                default: // True/False (1) или Spelling (3)
                    Console.WriteLine($"Play: Режим {mode} - без дополнительных данных");
                    break;
            }

            var questions = await query
                .OrderBy(q => q.Difficulty)
                .ToListAsync();

            if (!questions.Any())
            {
                TempData["Error"] = "В этой теме пока нет вопросов для выбранного режима";
                return RedirectToAction("SelectMode", new { topicId });
            }

            // Отладка
            Console.WriteLine($"Play: Найдено {questions.Count} вопросов для режима {mode}");
            if ((mode == 2 || mode == 4) && questions.Any())
            {
                var firstQ = questions.First();
                Console.WriteLine($"Первый вопрос: '{firstQ.Text}'");
                Console.WriteLine($"Количество Options: {firstQ.Options?.Count ?? 0}");
            }
            else if (mode == 5 && questions.Any())
            {
                var firstQ = questions.First();
                Console.WriteLine($"Первый вопрос: '{firstQ.Text}'");
                Console.WriteLine($"Количество SentenceParts: {firstQ.SentenceParts?.Count ?? 0}");
            }

            // Сохраняем список вопросов в сессии
            var questionIds = questions.Select(q => q.Id).ToList();
            HttpContext.Session.SetString("GameQuestions", JsonSerializer.Serialize(questionIds));
            HttpContext.Session.SetInt32("CurrentQuestionIndex", 0);
            HttpContext.Session.SetInt32("GameScore", 0);
            HttpContext.Session.SetInt32("GameMode", mode);
            HttpContext.Session.SetInt32("GameTopicId", topicId);

            var topicTitle = (await _context.Topics.FindAsync(topicId))?.Title ?? "Тема";

            var viewModel = new GameViewModel
            {
                TopicId = topicId,
                TopicTitle = topicTitle,
                CurrentQuestion = questions.First(),
                TotalQuestions = questions.Count,
                CurrentQuestionIndex = 0,
                Score = 0
            };

            return View("GameMode" + mode, viewModel);
        }

        // POST: /Game/CheckAnswer
        [HttpPost]
        public async Task<IActionResult> CheckAnswer([FromBody] QuestionResult result)
        {
            var childId = HttpContext.Session.GetInt32("ChildId");
            if (childId == null)
            {
                return Json(new { success = false, redirect = "/Account/ChildLogin" });
            }

            // Получаем данные из сессии
            var questionIdsJson = HttpContext.Session.GetString("GameQuestions");
            if (string.IsNullOrEmpty(questionIdsJson))
            {
                return Json(new { success = false, error = "Session expired" });
            }

            var questionIds = JsonSerializer.Deserialize<List<int>>(questionIdsJson);
            var currentIndex = HttpContext.Session.GetInt32("CurrentQuestionIndex") ?? 0;
            var score = HttpContext.Session.GetInt32("GameScore") ?? 0;
            var mode = HttpContext.Session.GetInt32("GameMode") ?? 1;
            var topicId = HttpContext.Session.GetInt32("GameTopicId") ?? 0;

            // Сохраняем ответ (всегда сохраняем, даже если неправильный)
            var userAnswer = new UserAnswer
            {
                ChildProfileId = childId.Value,
                QuestionId = result.QuestionId,
                IsCorrect = result.IsCorrect,
                AnswerTime = result.AnswerTime,
                AttemptDate = DateTime.Now
            };
            _context.UserAnswers.Add(userAnswer);

            // Обновляем счет (только если правильный ответ)
            if (result.IsCorrect)
            {
                score += 10;
                HttpContext.Session.SetInt32("GameScore", score);
            }

            await _context.SaveChangesAsync();

            // ========== НОВЫЙ КОД: Обновляем питомца ==========
            try
            {
                await UpdatePetStats(childId.Value, result.IsCorrect);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении питомца: {ex.Message}");
            }

            // ========== НОВЫЙ КОД: Проверяем достижения ==========
            try
            {
                await CheckAchievements(childId.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке достижений: {ex.Message}");
            }
            // ========== КОНЕЦ НОВОГО КОДА ==========

            // Увеличиваем индекс для следующего вопроса
            currentIndex++;
            HttpContext.Session.SetInt32("CurrentQuestionIndex", currentIndex);

            // Проверяем, есть ли еще вопросы
            if (currentIndex < questionIds.Count)
            {
                // Есть следующий вопрос
                var nextQuestionId = questionIds[currentIndex];
                Console.WriteLine($"CheckAnswer: Загружаем следующий вопрос ID {nextQuestionId} для режима {mode}");

                // Получаем следующий вопрос в зависимости от режима
                object questionDto = null;

                switch (mode)
                {
                    case 2: // Multiple Choice - нужны Options
                    case 4: // Translation - нужны Options
                        var nextQuestion = await _context.Questions
                            .Include(q => q.Options.OrderBy(o => o.Order))
                            .AsNoTracking()
                            .FirstOrDefaultAsync(q => q.Id == nextQuestionId);

                        if (nextQuestion == null)
                        {
                            Console.WriteLine($"CheckAnswer: Вопрос {nextQuestionId} не найден!");
                            return Json(new { success = false, error = "Question not found" });
                        }

                        Console.WriteLine($"CheckAnswer: Найден вопрос '{nextQuestion.Text}'");
                        Console.WriteLine($"CheckAnswer: Количество Options: {nextQuestion.Options?.Count ?? 0}");

                        if (nextQuestion.Options != null)
                        {
                            foreach (var opt in nextQuestion.Options.OrderBy(o => o.Order))
                            {
                                Console.WriteLine($"  - Option: '{opt.Text}', IsCorrect: {opt.IsCorrect}, Order: {opt.Order}");
                            }
                        }

                        questionDto = new
                        {
                            id = nextQuestion.Id,
                            text = nextQuestion.Text,
                            options = nextQuestion.Options.Select(o => new
                            {
                                id = o.Id,
                                text = o.Text,
                                isCorrect = o.IsCorrect,
                                order = o.Order
                            })
                        };
                        break;

                    case 5: // Sentence Builder - нужны SentenceParts
                        var nextSentenceQuestion = await _context.Questions
                            .Include(q => q.SentenceParts.OrderBy(s => s.WordOrder))
                            .AsNoTracking()
                            .FirstOrDefaultAsync(q => q.Id == nextQuestionId);

                        if (nextSentenceQuestion == null)
                        {
                            Console.WriteLine($"CheckAnswer: Вопрос {nextQuestionId} не найден!");
                            return Json(new { success = false, error = "Question not found" });
                        }

                        Console.WriteLine($"CheckAnswer: Найден вопрос '{nextSentenceQuestion.Text}'");
                        Console.WriteLine($"CheckAnswer: Количество SentenceParts: {nextSentenceQuestion.SentenceParts?.Count ?? 0}");

                        questionDto = new
                        {
                            id = nextSentenceQuestion.Id,
                            text = nextSentenceQuestion.Text,
                            sentenceParts = nextSentenceQuestion.SentenceParts.Select(s => new
                            {
                                id = s.Id,
                                wordText = s.WordText,
                                wordOrder = s.WordOrder,
                                isPunctuation = s.IsPunctuation
                            })
                        };
                        break;

                    default: // True/False (1) или Spelling (3)
                        var nextSimpleQuestion = await _context.Questions
                            .AsNoTracking()
                            .FirstOrDefaultAsync(q => q.Id == nextQuestionId);

                        if (nextSimpleQuestion == null)
                        {
                            return Json(new { success = false, error = "Question not found" });
                        }

                        questionDto = new
                        {
                            id = nextSimpleQuestion.Id,
                            text = nextSimpleQuestion.Text,
                            isTrueStatement = nextSimpleQuestion.IsTrueStatement
                        };
                        break;
                }

                return Json(new
                {
                    success = true,
                    hasNext = true,
                    question = questionDto,
                    score = score,
                    currentIndex = currentIndex,
                    totalQuestions = questionIds.Count
                });
            }
            else
            {
                // Игра закончена - считаем звезды
                var maxScore = questionIds.Count * 10;
                var percentage = (score * 100.0) / maxScore;
                int stars = 0;

                if (percentage >= 90) stars = 3;
                else if (percentage >= 70) stars = 2;
                else if (percentage >= 50) stars = 1;

                Console.WriteLine($"Game finished! Score: {score}/{maxScore}, Stars: {stars}");

                // Сохраняем результат теста
                var testResult = new TestResult
                {
                    ChildProfileId = childId.Value,
                    TopicId = topicId,
                    GameMode = (GameMode)mode,
                    Score = score,
                    MaxScore = maxScore,
                    StarsEarned = stars,
                    CompletionDate = DateTime.Now
                };
                _context.TestResults.Add(testResult);

                // Обновляем прогресс
                await UpdateProgress(childId.Value, topicId, result.IsCorrect);

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    hasNext = false,
                    score = score,
                    maxScore = maxScore,
                    stars = stars,
                    redirect = Url.Action("GameResult", "Game", new { topicId, score, stars })
                });
            }
        }

        // GET: /Game/GameResult
        public IActionResult GameResult(int topicId, int score, int stars)
        {
            ViewBag.TopicId = topicId;
            ViewBag.Score = score;
            ViewBag.Stars = stars;
            return View();
        }

        private async Task UpdateProgress(int childId, int topicId, bool isCorrect)
        {
            var progress = await _context.Progresses
                .FirstOrDefaultAsync(p => p.ChildProfileId == childId && p.TopicId == topicId);

            if (progress == null)
            {
                progress = new Progress
                {
                    ChildProfileId = childId,
                    TopicId = topicId,
                    CorrectAnswers = isCorrect ? 1 : 0,
                    TotalQuestions = 1,
                    LastAccessed = DateTime.Now,
                    IsCompleted = false
                };
                _context.Progresses.Add(progress);
            }
            else
            {
                progress.TotalQuestions++;
                if (isCorrect) progress.CorrectAnswers++;
                progress.LastAccessed = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        // ========== НОВЫЙ МЕТОД: Обновление статистики питомца ==========
        private async Task UpdatePetStats(int childId, bool isCorrect)
        {
            var pet = await _context.VirtualPets
                .FirstOrDefaultAsync(p => p.ChildProfileId == childId);

            if (pet != null)
            {
                if (isCorrect)
                {
                    pet.Happiness = Math.Min(100, pet.Happiness + 3);
                    pet.Experience += 2;
                }
                else
                {
                    pet.Happiness = Math.Max(0, pet.Happiness - 1);
                }

                // Постепенное уменьшение показателей
                pet.Food = Math.Max(0, pet.Food - 1);
                pet.Energy = Math.Max(0, pet.Energy - 1);

                // Проверка на повышение уровня
                if (pet.Experience >= pet.Level * 100)
                {
                    pet.Level++;
                    pet.Experience = 0;
                }

                await _context.SaveChangesAsync();
            }
        }

        // ========== НОВЫЙ МЕТОД: Проверка достижений ==========
        private async Task CheckAchievements(int childId)
        {
            // Получаем все ответы ребенка
            var answers = await _context.UserAnswers
                .Where(a => a.ChildProfileId == childId)
                .OrderByDescending(a => a.AttemptDate)
                .ToListAsync();

            // Получаем прогресс по темам
            var progresses = await _context.Progresses
                .Where(p => p.ChildProfileId == childId)
                .ToListAsync();

            // Получаем уже полученные достижения
            var earnedAchievementIds = await _context.EarnedAchievements
                .Where(e => e.ChildProfileId == childId)
                .Select(e => e.AchievementId)
                .ToListAsync();

            // Получаем все достижения
            var achievements = await _context.Achievements.ToListAsync();

            foreach (var achievement in achievements)
            {
                // Проверяем, не получено ли уже это достижение
                if (earnedAchievementIds.Contains(achievement.Id))
                    continue;

                bool earned = false;

                switch (achievement.Title)
                {
                    case "Первые шаги":
                        earned = answers.Count >= 10;
                        break;
                    case "Юный лингвист":
                        earned = answers.Count >= 50;
                        break;
                    case "Знаток грамматики":
                        earned = answers.Count >= 100;
                        break;
                    case "Скороход":
                        // Проверяем серию из 5 правильных ответов подряд
                        earned = HasCorrectStreak(answers, 5);
                        break;
                    case "Непобедимый":
                        earned = HasCorrectStreak(answers, 10);
                        break;
                    case "Золотая звезда":
                        earned = progresses.Any(p => p.StarsEarned >= 3);
                        break;
                    case "Коллекционер":
                        earned = progresses.Count >= 5;
                        break;
                    case "Исследователь":
                        earned = progresses.Count >= 10;
                        break;
                    case "Суперзвезда":
                        earned = progresses.Count(p => p.StarsEarned >= 3) >= 5;
                        break;
                }

                if (earned)
                {
                    _context.EarnedAchievements.Add(new EarnedAchievement
                    {
                        ChildProfileId = childId,
                        AchievementId = achievement.Id,
                        EarnedDate = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        // ========== НОВЫЙ МЕТОД: Проверка серии правильных ответов ==========
        private bool HasCorrectStreak(List<UserAnswer> answers, int streakLength)
        {
            if (answers.Count < streakLength)
                return false;

            int currentStreak = 0;
            foreach (var answer in answers.Take(50)) // Проверяем последние 50 ответов
            {
                if (answer.IsCorrect)
                {
                    currentStreak++;
                    if (currentStreak >= streakLength)
                        return true;
                }
                else
                {
                    currentStreak = 0;
                }
            }
            return false;
        }
    }
}