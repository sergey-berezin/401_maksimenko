using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using static Task4.Controllers.TextAnswererController;
using Task4.Models;
using Xunit;
namespace WebServerTest
{
    public class Tests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;
        public Tests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }


        [Fact]
        public async Task GetAnswersManyQuestionsTest()
        {
            var client = factory.CreateClient();

            string text = File.ReadAllText("..\\..\\..\\hobbit.txt");

            QI question1 = new QI("what is this story about?", "answer1");
            QI question2 = new QI("what did hobbit live?", "answer2");
            List<QI> questions = new List<QI> { question1, question2};
            QuestionsAndText request = new QuestionsAndText(
                text,
                questions
            );

            var inputJson = JsonConvert.SerializeObject(request);
            var response = await client.PostAsJsonAsync("api/textQuestionAnswerer", request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var answersJson = await response.Content.ReadAsStringAsync();
            var answersObjects = JsonConvert.DeserializeObject<List<Answers>>(answersJson);
            Assert.Equal(questions.Count, answersObjects.Count);
            for (int i = 0; i < answersObjects.Count; i++)
            {
                Assert.Equal(questions[i].answerId, answersObjects[i].Id);
            }
        }

        [Fact]
        public async Task GetAnswersOneQuestionTest()
        {
            var client = factory.CreateClient();

            string text = File.ReadAllText("..\\..\\..\\hobbit.txt");

            QI question1 = new QI("where did hobbit lived?", "ans1");
            List<string> answers = new List<string> { "in a hole in the ground" };
            List<QI> questions = new List<QI> { question1 };
            QuestionsAndText request = new QuestionsAndText(
                text,
                questions
            );

            var inputJson = JsonConvert.SerializeObject(request);
            var response = await client.PostAsJsonAsync("api/textQuestionAnswerer", request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var answersJson = await response.Content.ReadAsStringAsync();
            var answersObjects = JsonConvert.DeserializeObject<List<Answers>>(answersJson);
            Assert.Equal(questions.Count, answersObjects.Count);
            for (int i = 0; i < answersObjects.Count; i++)
            {
                Assert.Equal(questions[i].answerId, answersObjects[i].Id);
                Assert.Equal(answers[i], answersObjects[i].Answer);
            }
        }

        [Fact]
        public async Task GetAnswersEmptyQuestionTest()
        {
            var client = factory.CreateClient();

            string text = File.ReadAllText("..\\..\\..\\hobbit.txt");

            QI question1 = new QI("", "ans1");
            List<QI> questions = new List<QI> { question1 };
            QuestionsAndText request = new QuestionsAndText(
                text,
                questions
            );

            var inputJson = JsonConvert.SerializeObject(request);
            var response = await client.PostAsJsonAsync("api/textQuestionAnswerer", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var answersJson = await response.Content.ReadAsStringAsync();
            Assert.Equal("Empty", answersJson);
        }

        [Fact]
        public async Task GetAnswersEmptyTextTest()
        {
            var client = factory.CreateClient();

            string text = "";

            QI question1 = new QI("what is it?", "ans1");
            List<QI> questions = new List<QI> { question1 };
            QuestionsAndText request = new QuestionsAndText(
                text,
                questions
            );

            var inputJson = JsonConvert.SerializeObject(request);
            var response = await client.PostAsJsonAsync("api/textQuestionAnswerer", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var answersJson = await response.Content.ReadAsStringAsync();
            Assert.Equal("Empty", answersJson);
        }
    }
}