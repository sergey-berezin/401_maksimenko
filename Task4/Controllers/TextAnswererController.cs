using NuGetBertSpace;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Text.Json.Nodes;
using Task4.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Task4.Controllers
{
    [Route("api/textQuestionAnswerer")]
    [ApiController]
    public class TextAnswererController : Controller
    {
        private NuGetBert bertModelService;
        private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private CancellationToken token;

        public TextAnswererController(NuGetBert bertModelService)
        {
            this.bertModelService = bertModelService;
            token = cancelTokenSource.Token;
        }

        [HttpPost]
        public async Task<IActionResult> GetAnswers([FromBody] QuestionsAndText request)
        {
            string text = request.text;
            List<QI> QuestionsId = request.QuestionsId;
            List<Task<Answers>> Tasks = new List<Task<Answers>>();
            foreach (var item in QuestionsId)
            {
                string question = item.question;
                string Id = item.answerId;
                var sentence = $"{{\"question\": \"{question}\", \"context\": \"{text}\"}}";
                Tasks.Add(BertAsync(bertModelService, sentence, Id, token));
            }
            var answerResponses = await Task.WhenAll(Tasks);
            return Ok(answerResponses);

        }
        
        public async Task<Answers> BertAsync(NuGetBert bertModel, string sentence, string Id, CancellationToken token)
        {
            var answer = await bertModel.AnswerBert(sentence, token);
            return new Answers(Id, answer.ToString());
        }
    }
}
