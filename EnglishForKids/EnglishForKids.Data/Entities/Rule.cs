using System.Collections.Generic;

namespace EnglishForKids.Data.Entities
{
    public class Rule
    {
        public int Id { get; set; }
        public int TopicId { get; set; }
        public Topic Topic { get; set; }
        public string Title { get; set; }
        public string TheoryText { get; set; }
        public string ExampleText { get; set; }
        public string VideoUrl { get; set; }

        public ICollection<Question> Questions { get; set; }
    }
}