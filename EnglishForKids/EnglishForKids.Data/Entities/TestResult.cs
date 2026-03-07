using System;
using EnglishForKids.Data.Enums;

namespace EnglishForKids.Data.Entities
{
    public class TestResult
    {
        public int Id { get; set; }
        public int ChildProfileId { get; set; }
        public ChildProfile Child { get; set; }
        public int TopicId { get; set; }
        public Topic Topic { get; set; }
        public GameMode GameMode { get; set; }
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public int StarsEarned { get; set; }
        public DateTime CompletionDate { get; set; }
    }
}