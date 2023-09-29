using ConsoleApp1;
 

namespace MyApp 
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var hobbit = File.ReadAllText(args[0]);
            string modelFilePath = "bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
            Task result = Process(modelFilePath, hobbit);
        }

        static async Task Process(string modelFilePath, string hobbit)
        {
            var ct = new CancellationTokenSource();
            var cancel = ct.Token;
            var nugetResult = new NuGetBert(modelFilePath, cancel);
            var tasks = new List<Task>();
            var download_res = nugetResult.download();
            
            while (!cancel.IsCancellationRequested)
            {
                Console.Write("Ask me smth: ");
                string question = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(question))
                    ct.Cancel();
                var sentence = $"{{\"question\": {question}, \"context\": \"@CTX\"}}".Replace("@CTX", hobbit);
                //Console.WriteLine(question);
                var task = nugetResult.TokenizeText(sentence).ContinueWith(t =>
                {
                    Console.WriteLine(t.Result);
                });
                tasks.Add(task);

            }
            await Task.WhenAll(tasks.ToArray());
            
            
        }

    }


}
