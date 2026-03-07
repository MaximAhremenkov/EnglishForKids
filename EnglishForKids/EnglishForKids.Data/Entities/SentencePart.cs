namespace EnglishForKids.Data.Entities
{
    public class SentencePart
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public string WordText { get; set; }
        public int WordOrder { get; set; }
        public bool IsPunctuation { get; set; }
    }
}