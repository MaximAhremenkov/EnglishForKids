using EnglishForKids.Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EnglishForKids.Web.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalChildren { get; set; }
        public int TotalTopics { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalAnswers { get; set; }
        public List<Topic> RecentTopics { get; set; }
        public List<ChildProfile> RecentChildren { get; set; }
    }

    public class TopicAdminViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название темы")]
        [Display(Name = "Название темы")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Введите описание темы")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Категория")]
        public int CategoryId { get; set; }

        [Display(Name = "Порядок отображения")]
        public int Order { get; set; }

        [Display(Name = "URL иконки")]
        public string IconUrl { get; set; }

        public List<Category> Categories { get; set; }
    }

    public class QuestionAdminViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Выберите тему")]
        [Display(Name = "Тема")]
        public int TopicId { get; set; }

        [Required(ErrorMessage = "Выберите тип вопроса")]
        [Display(Name = "Тип вопроса")]
        public int QuestionType { get; set; }

        [Required(ErrorMessage = "Введите текст вопроса")]
        [Display(Name = "Текст вопроса")]
        public string Text { get; set; }

        [Display(Name = "URL изображения")]
        public string ImageUrl { get; set; }

        [Display(Name = "URL аудио")]
        public string AudioUrl { get; set; }

        [Display(Name = "Сложность")]
        public int Difficulty { get; set; }

        [Display(Name = "Правильный ответ (для True/False)")]
        public bool? IsTrueStatement { get; set; }

        public List<Topic> Topics { get; set; }
    }

    public class OptionAdminViewModel
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Введите текст варианта")]
        [Display(Name = "Текст варианта")]
        public string Text { get; set; }

        [Display(Name = "Правильный ответ")]
        public bool IsCorrect { get; set; }

        [Display(Name = "Порядок")]
        public int Order { get; set; }
    }

    public class RuleAdminViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Выберите тему")]
        [Display(Name = "Тема")]
        public int TopicId { get; set; }

        [Required(ErrorMessage = "Введите название правила")]
        [Display(Name = "Название правила")]
        public string Title { get; set; }

        [Display(Name = "Текст теории")]
        public string TheoryText { get; set; }

        [Display(Name = "Примеры")]
        public string ExampleText { get; set; }

        [Display(Name = "URL видео (YouTube)")]
        public string VideoUrl { get; set; }

        public List<Topic> Topics { get; set; }
    }
}