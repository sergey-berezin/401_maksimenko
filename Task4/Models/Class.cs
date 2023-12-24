namespace Task4.Models
{
        public class QuestionsAndText
        {
            public string text { get; }
            public List<QI> QuestionsId { get; }
            public QuestionsAndText(string text, List<QI> QuestionsId)
            {
                this.text = text;
                this.QuestionsId = QuestionsId;
            }
        }
        public class QI
        {
            public string question { get; }
            public string answerId { get; }

            public QI(string answerId, string question)
            {
                this.answerId = answerId;
                this.question = question;
            }
        }

        public class Answers
        {
            public string answerId { get; }
            public string Answer { get; }

            public Answers(string answerId, string answer)
            {
                this.answerId = answerId;
                this.Answer = answer;
            }
        }

}
