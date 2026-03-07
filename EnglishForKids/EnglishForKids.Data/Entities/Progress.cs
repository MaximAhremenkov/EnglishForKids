using System;

namespace EnglishForKids.Data.Entities
{
    public class Progress
    {
        public int Id { get; set; }
        public int ChildProfileId { get; set; }
        public ChildProfile Child { get; set; }
        public int TopicId { get; set; }
        public Topic Topic { get; set; }
        public int StarsEarned { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime LastAccessed { get; set; }
        public bool IsCompleted { get; set; }
    }
}