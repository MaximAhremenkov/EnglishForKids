using EnglishForKids.Data.Entities;
using System.Collections.Generic;

namespace EnglishForKids.Web.ViewModels
{
    public class GameViewModel
    {
        public int TopicId { get; set; }
        public string TopicTitle { get; set; }
        public Question CurrentQuestion { get; set; }
        public int TotalQuestions { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public int Score { get; set; }
        public int StarsEarned { get; set; }
    }

    public class QuestionResult
    {
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; }
        public int AnswerTime { get; set; }
    }
}