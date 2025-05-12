namespace RemediEmr.Data.Class
{
    public class FeedbackSubmission
    {
        public string PatiId { get; set; }
        public int QuestionsId { get; set; }
        public int AnswersId { get; set; }
        public int MasterFqapId { get; set; }
        public string TxtAnswers { get; set; }
    }

}
