using System.Collections.Generic;
using EnglishForKids.Data.Entities.Identity;

namespace EnglishForKids.Data.Entities
{
    public class ChildProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Grade { get; set; }
        public string AvatarUrl { get; set; }
        public string PinCode { get; set; }
        public string ParentId { get; set; }
        public AppUser Parent { get; set; }
        public ICollection<Progress> Progresses { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; }
        public ICollection<TestResult> TestResults { get; set; }
        public VirtualPet VirtualPet { get; set; }
    }
}