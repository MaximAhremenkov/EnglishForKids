using System.Collections.Generic;

namespace EnglishForKids.Data.Entities
{
    public class Topic
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int Order { get; set; }
        public string IconUrl { get; set; }

        public ICollection<Rule> Rules { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<Progress> Progresses { get; set; }
    }
}