using System.Collections.Generic;

namespace EnglishForKids.Data.Entities
{
    public class Achievement
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public int RequiredCount { get; set; }

        public ICollection<EarnedAchievement> EarnedAchievements { get; set; }
    }
}