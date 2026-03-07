using EnglishForKids.Data.Entities;
using EnglishForKids.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace EnglishForKids.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Если база уже заполнена - выходим
                if (context.Topics.Any())
                {
                    return;
                }

                // 1. Создаем категории
                var categories = CreateCategories(context);

                // 2. Создаем темы
                var topics = CreateTopics(context, categories);

                // 3. Создаем правила
                var rules = CreateRules(context, topics);

                // 4. Создаем вопросы
                CreateQuestions(context, topics, rules);

                // 5. Создаем достижения
                CreateAchievements(context);

                Console.WriteLine("База данных успешно заполнена!");
            }
        }

        private static Category[] CreateCategories(ApplicationDbContext context)
        {
            var categories = new Category[]
            {
                new Category { Name = "Грамматика" },
                new Category { Name = "Чтение" },
                new Category { Name = "Лексика" }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return categories;
        }

        private static Topic[] CreateTopics(ApplicationDbContext context, Category[] categories)
        {
            var grammarCat = categories.First(c => c.Name == "Грамматика");
            var readingCat = categories.First(c => c.Name == "Чтение");
            var vocabCat = categories.First(c => c.Name == "Лексика");

            var topics = new Topic[]
            {
                // Грамматика
                new Topic
                {
                    Title = "Глагол to be",
                    Description = "Учимся правильно использовать am, is, are",
                    CategoryId = grammarCat.Id,
                    Order = 1,
                    IconUrl = "/images/topics/tobe.png"
                },
                new Topic
                {
                    Title = "Артикли a/an/the",
                    Description = "Когда использовать a, an и the",
                    CategoryId = grammarCat.Id,
                    Order = 2,
                    IconUrl = "/images/topics/articles.png"
                },
                new Topic
                {
                    Title = "Множественное число",
                    Description = "Учимся образовывать множественное число существительных",
                    CategoryId = grammarCat.Id,
                    Order = 3,
                    IconUrl = "/images/topics/plural.png"
                },
                new Topic
                {
                    Title = "Present Simple",
                    Description = "Простое настоящее время",
                    CategoryId = grammarCat.Id,
                    Order = 4,
                    IconUrl = "/images/topics/presentsimple.png"
                },
                new Topic
                {
                    Title = "Present Continuous",
                    Description = "Настоящее длительное время",
                    CategoryId = grammarCat.Id,
                    Order = 5,
                    IconUrl = "/images/topics/presentcontinuous.png"
                },
                new Topic
                {
                    Title = "Местоимения",
                    Description = "Личные и притяжательные местоимения",
                    CategoryId = grammarCat.Id,
                    Order = 6,
                    IconUrl = "/images/topics/pronouns.png"
                },
                new Topic
                {
                    Title = "Предлоги места",
                    Description = "in, on, under, behind, next to",
                    CategoryId = grammarCat.Id,
                    Order = 7,
                    IconUrl = "/images/topics/prepositions.png"
                },
                new Topic
                {
                    Title = "Конструкция there is/there are",
                    Description = "Говорим о том, что где находится",
                    CategoryId = grammarCat.Id,
                    Order = 8,
                    IconUrl = "/images/topics/thereis.png"
                },
                new Topic
                {
                    Title = "Модальный глагол can",
                    Description = "Учимся говорить о своих умениях",
                    CategoryId = grammarCat.Id,
                    Order = 9,
                    IconUrl = "/images/topics/can.png"
                },
                
                // Чтение
                new Topic
                {
                    Title = "Алфавит",
                    Description = "Изучаем буквы и их произношение",
                    CategoryId = readingCat.Id,
                    Order = 1,
                    IconUrl = "/images/topics/alphabet.png"
                },
                new Topic
                {
                    Title = "Дифтонги ow/ou",
                    Description = "Учимся читать сочетания ow и ou",
                    CategoryId = readingCat.Id,
                    Order = 2,
                    IconUrl = "/images/topics/owou.png"
                },
                new Topic
                {
                    Title = "Дифтонги oi/oy",
                    Description = "Учимся читать сочетания oi и oy",
                    CategoryId = readingCat.Id,
                    Order = 3,
                    IconUrl = "/images/topics/oioy.png"
                },
                new Topic
                {
                    Title = "Дифтонги ea/ee",
                    Description = "Учимся читать сочетания ea и ee",
                    CategoryId = readingCat.Id,
                    Order = 4,
                    IconUrl = "/images/topics/eatee.png"
                },
                new Topic
                {
                    Title = "Открытый и закрытый слог",
                    Description = "Учимся правильно читать гласные",
                    CategoryId = readingCat.Id,
                    Order = 5,
                    IconUrl = "/images/topics/syllables.png"
                },
                
                // Лексика
                new Topic
                {
                    Title = "Семья",
                    Description = "Мама, папа, брат, сестра",
                    CategoryId = vocabCat.Id,
                    Order = 1,
                    IconUrl = "/images/topics/family.png"
                },
                new Topic
                {
                    Title = "Животные",
                    Description = "Домашние и дикие животные",
                    CategoryId = vocabCat.Id,
                    Order = 2,
                    IconUrl = "/images/topics/animals.png"
                },
                new Topic
                {
                    Title = "Цвета",
                    Description = "Изучаем цвета",
                    CategoryId = vocabCat.Id,
                    Order = 3,
                    IconUrl = "/images/topics/colors.png"
                },
                new Topic
                {
                    Title = "Еда",
                    Description = "Фрукты, овощи, напитки",
                    CategoryId = vocabCat.Id,
                    Order = 4,
                    IconUrl = "/images/topics/food.png"
                },
                new Topic
                {
                    Title = "Игрушки",
                    Description = "Названия игрушек",
                    CategoryId = vocabCat.Id,
                    Order = 5,
                    IconUrl = "/images/topics/toys.png"
                }
            };

            context.Topics.AddRange(topics);
            context.SaveChanges();

            return topics;
        }

        private static Rule[] CreateRules(ApplicationDbContext context, Topic[] topics)
        {
            var tobeTopic = topics.First(t => t.Title.Contains("to be"));
            var articlesTopic = topics.First(t => t.Title.Contains("Артикли"));
            var pluralTopic = topics.First(t => t.Title.Contains("Множественное"));

            var rules = new Rule[]
            {
                // Правила для глагола to be
                new Rule
                {
                    TopicId = tobeTopic.Id,
                    Title = "Я, ты, мы - am, is, are",
                    TheoryText = "В английском языке глагол to be меняется в зависимости от того, кто совершает действие:\n\n" +
                                "I **am** - Я есть\n" +
                                "He/She/It **is** - Он/она/оно есть\n" +
                                "You/We/They **are** - Ты/мы/они есть",
                    ExampleText = "I am a student.\nShe is happy.\nWe are friends."
                },
                // Правила для артиклей
                new Rule
                {
                    TopicId = articlesTopic.Id,
                    Title = "A, an или the?",
                    TheoryText = "A - ставим перед словами, начинающимися с согласного звука\n" +
                                "An - ставим перед словами, начинающимися с гласного звука\n" +
                                "The - говорим о чем-то конкретном",
                    ExampleText = "a cat, an apple, the sun"
                },
                // Правила для множественного числа
                new Rule
                {
                    TopicId = pluralTopic.Id,
                    Title = "Как сделать множественное число",
                    TheoryText = "Обычно добавляем -s\n" +
                                "Если слово заканчивается на -s, -ss, -sh, -ch, -x, -o, добавляем -es\n" +
                                "Если слово заканчивается на согласную + y, меняем y на ies",
                    ExampleText = "cat → cats, bus → buses, baby → babies"
                }
            };

            context.Rules.AddRange(rules);
            context.SaveChanges();

            return rules;
        }

        private static void CreateQuestions(ApplicationDbContext context, Topic[] topics, Rule[] rules)
        {
            var tobeTopic = topics.First(t => t.Title.Contains("to be"));
            var tobeRule = rules.First(r => r.TopicId == tobeTopic.Id);

            // Вопросы для True/False
            var trueFalseQuestions = new Question[]
            {
                new Question
                {
                    TopicId = tobeTopic.Id,
                    RuleId = tobeRule.Id,
                    QuestionType = QuestionType.TrueFalse,
                    Text = "I am a student",
                    Difficulty = DifficultyLevel.Easy,
                    IsTrueStatement = true
                },
                new Question
                {
                    TopicId = tobeTopic.Id,
                    RuleId = tobeRule.Id,
                    QuestionType = QuestionType.TrueFalse,
                    Text = "She are happy",
                    Difficulty = DifficultyLevel.Easy,
                    IsTrueStatement = false
                },
                new Question
                {
                    TopicId = tobeTopic.Id,
                    RuleId = tobeRule.Id,
                    QuestionType = QuestionType.TrueFalse,
                    Text = "We is friends",
                    Difficulty = DifficultyLevel.Easy,
                    IsTrueStatement = false
                },
                new Question
                {
                    TopicId = tobeTopic.Id,
                    RuleId = tobeRule.Id,
                    QuestionType = QuestionType.TrueFalse,
                    Text = "He is a teacher",
                    Difficulty = DifficultyLevel.Easy,
                    IsTrueStatement = true
                },
                new Question
                {
                    TopicId = tobeTopic.Id,
                    RuleId = tobeRule.Id,
                    QuestionType = QuestionType.TrueFalse,
                    Text = "You am my friend",
                    Difficulty = DifficultyLevel.Easy,
                    IsTrueStatement = false
                }
            };

            context.Questions.AddRange(trueFalseQuestions);
            context.SaveChanges();

            // Вопросы для Multiple Choice
            var mcQuestions = new Question[]
            {
                new Question
                {
                    TopicId = tobeTopic.Id,
                    RuleId = tobeRule.Id,
                    QuestionType = QuestionType.MultipleChoice,
                    Text = "Как правильно сказать 'Я ученик'?",
                    Difficulty = DifficultyLevel.Easy
                },
                new Question
                {
                    TopicId = tobeTopic.Id,
                    RuleId = tobeRule.Id,
                    QuestionType = QuestionType.MultipleChoice,
                    Text = "Выбери правильный перевод: 'Она счастлива'",
                    Difficulty = DifficultyLevel.Easy
                },
                new Question
                {
                    TopicId = tobeTopic.Id,
                    RuleId = tobeRule.Id,
                    QuestionType = QuestionType.MultipleChoice,
                    Text = "Как будет 'Мы друзья'?",
                    Difficulty = DifficultyLevel.Easy
                }
            };

            context.Questions.AddRange(mcQuestions);
            context.SaveChanges();

            // Добавляем варианты ответов для Multiple Choice
            var mcQuestion1 = mcQuestions[0];
            var options1 = new QuestionOption[]
            {
                new QuestionOption { QuestionId = mcQuestion1.Id, Text = "I am student", IsCorrect = true, Order = 1 },
                new QuestionOption { QuestionId = mcQuestion1.Id, Text = "I is student", IsCorrect = false, Order = 2 },
                new QuestionOption { QuestionId = mcQuestion1.Id, Text = "I are student", IsCorrect = false, Order = 3 },
                new QuestionOption { QuestionId = mcQuestion1.Id, Text = "Me student", IsCorrect = false, Order = 4 }
            };

            context.QuestionOptions.AddRange(options1);

            var mcQuestion2 = mcQuestions[1];
            var options2 = new QuestionOption[]
            {
                new QuestionOption { QuestionId = mcQuestion2.Id, Text = "She is happy", IsCorrect = true, Order = 1 },
                new QuestionOption { QuestionId = mcQuestion2.Id, Text = "She are happy", IsCorrect = false, Order = 2 },
                new QuestionOption { QuestionId = mcQuestion2.Id, Text = "She am happy", IsCorrect = false, Order = 3 },
                new QuestionOption { QuestionId = mcQuestion2.Id, Text = "Her happy", IsCorrect = false, Order = 4 }
            };

            context.QuestionOptions.AddRange(options2);

            var mcQuestion3 = mcQuestions[2];
            var options3 = new QuestionOption[]
            {
                new QuestionOption { QuestionId = mcQuestion3.Id, Text = "We are friends", IsCorrect = true, Order = 1 },
                new QuestionOption { QuestionId = mcQuestion3.Id, Text = "We is friends", IsCorrect = false, Order = 2 },
                new QuestionOption { QuestionId = mcQuestion3.Id, Text = "We am friends", IsCorrect = false, Order = 3 },
                new QuestionOption { QuestionId = mcQuestion3.Id, Text = "Us friends", IsCorrect = false, Order = 4 }
            };

            context.QuestionOptions.AddRange(options3);
            context.SaveChanges();

            // Вопросы для Spelling (вставь букву)
            var spellingQuestions = new Question[]
            {
                new Question
                {
                    TopicId = topics.First(t => t.Title.Contains("Алфавит")).Id,
                    QuestionType = QuestionType.Spelling,
                    Text = "C_T (кот)",
                    Difficulty = DifficultyLevel.Easy
                },
                new Question
                {
                    TopicId = topics.First(t => t.Title.Contains("Алфавит")).Id,
                    QuestionType = QuestionType.Spelling,
                    Text = "D_G (собака)",
                    Difficulty = DifficultyLevel.Easy
                }
            };

            context.Questions.AddRange(spellingQuestions);
            context.SaveChanges();
        }

        private static void CreateAchievements(ApplicationDbContext context)
        {
            var achievements = new Achievement[]
            {
                new Achievement
                {
                    Title = "Первые шаги",
                    Description = "Правильно ответить на 10 вопросов",
                    IconUrl = "/images/achievements/first-steps.png",
                    RequiredCount = 10
                },
                new Achievement
                {
                    Title = "Знаток грамматики",
                    Description = "Изучить 5 тем по грамматике",
                    IconUrl = "/images/achievements/grammar.png",
                    RequiredCount = 5
                },
                new Achievement
                {
                    Title = "Скорочтение",
                    Description = "Правильно ответить на 20 вопросов по чтению",
                    IconUrl = "/images/achievements/reading.png",
                    RequiredCount = 20
                },
                new Achievement
                {
                    Title = "Золотая звезда",
                    Description = "Получить 3 звезды в любой теме",
                    IconUrl = "/images/achievements/gold-star.png",
                    RequiredCount = 3
                }
            };

            context.Achievements.AddRange(achievements);
            context.SaveChanges();
        }
    }
}