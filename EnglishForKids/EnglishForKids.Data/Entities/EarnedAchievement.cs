using System;

namespace EnglishForKids.Data.Entities
{
    public class EarnedAchievement
    {
        public int Id { get; set; }
        public int ChildProfileId { get; set; }
        public ChildProfile Child { get; set; }
        public int AchievementId { get; set; }
        public Achievement Achievement { get; set; }
        public DateTime EarnedDate { get; set; }
    }
}