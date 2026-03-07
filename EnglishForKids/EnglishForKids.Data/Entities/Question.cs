using System.Collections.Generic;
using EnglishForKids.Data.Enums;

namespace EnglishForKids.Data.Entities
{
    public class Question
    {
        public int Id { get; set; }
        public int RuleId { get; set; }
        public Rule Rule { get; set; }
        public int? TopicId { get; set; }
        public Topic Topic { get; set; }

        public QuestionType QuestionType { get; set; }
        public string Text { get; set; }
        public string ImageUrl { get; set; }
        public string AudioUrl { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public bool? IsTrueStatement { get; set; }

        public ICollection<QuestionOption> Options { get; set; }
        public ICollection<SentencePart> SentenceParts { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; }
    }
}