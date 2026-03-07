using System;

namespace EnglishForKids.Data.Entities
{
    public class UserAnswer
    {
        public int Id { get; set; }
        public int ChildProfileId { get; set; }
        public ChildProfile Child { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public bool IsCorrect { get; set; }
        public int AnswerTime { get; set; }
        public DateTime AttemptDate { get; set; }
    }
}