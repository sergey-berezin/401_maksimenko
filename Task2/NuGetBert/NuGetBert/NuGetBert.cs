using BERTTokenizers;
using Microsoft.ML.Data;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using System.Windows;

namespace NuGetBertSpace
{
    public class NuGetBert
    {
        public NuGetBert(string modelFilePath, CancellationToken cancelToken)
        {
            ModelPath = modelFilePath;
            cancel = cancelToken;
        }
        public static string ModelPath;
        CancellationToken cancel;
        private static InferenceSession session;

        public async Task Download()
        {
            
            try
            {
                await DownloadModelFileAsync();
                session = new InferenceSession(ModelPath);
            }
            catch (Exception ex)
            {
                throw new Exception("DownloadModelFileAsync", ex);
            }
        }

        public static Task DownloadModelFileAsync()
        {
            return Task.Run(async () =>
            {
                // проверка наличия файла с нейронной сетью
                if (!File.Exists(ModelPath))
                {
                    // скачивание файла с нейронной сетью
                    string modelUrl = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
                    int retryCount = 3;
                    bool downloaded = false;
                    int retries = 0;
                    long totalBytesRead = 0;
                    const int bufferSize = 8192;

                    while (!downloaded && retries < retryCount)
                    {
                        try
                        {
                            using (var client = new HttpClient())
                            {
                                HttpResponseMessage response = await client.GetAsync(modelUrl, HttpCompletionOption.ResponseHeadersRead);
                                response.EnsureSuccessStatusCode();

                                using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                                              fileStream = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true))
                                {
                                    byte[] buffer = new byte[bufferSize];
                                    int bytesRead;
                                    do
                                    {
                                        bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                                        totalBytesRead += bytesRead;
                                        //Console.WriteLine($"Downloaded {totalBytesRead} bytes.");

                                    } while (bytesRead > 0);
                                }
                            }
                            downloaded = true;
                        }
                        catch (HttpRequestException)
                        {
                            retries++;
                        }

                    }
                    if (!downloaded)
                    {
                        throw new Exception($"Failed to download data from {modelUrl} after {retryCount} retries.");
                    }
                    //Console.WriteLine($"Downloaded {totalBytesRead} bytes.");
                }
            });
        }
        public async Task<string> AnswerBert(string sentence, CancellationToken cancelToken)
        {
            return await Task<string>.Factory.StartNew(() =>
            {
                try
                {
                    cancel = cancelToken;
                    cancel.ThrowIfCancellationRequested();

                    // Create Tokenizer and tokenize the sentence.
                    var tokenizer = new BertUncasedLargeTokenizer();

                    // Get the sentence tokens.
                    var tokens = tokenizer.Tokenize(sentence);
                    //Console.WriteLine(String.Join(", ", tokens));

                    // Encode the sentence and pass in the count of the tokens in the sentence.
                    var encoded = tokenizer.Encode(tokens.Count(), sentence);

                    // Break out encoding to InputIds, AttentionMask and TypeIds from list of (input_id, attention_mask, type_id).
                    var bertInput = new BertInput()
                    {
                        InputIds = encoded.Select(t => t.InputIds).ToArray(),
                        AttentionMask = encoded.Select(t => t.AttentionMask).ToArray(),
                        TypeIds = encoded.Select(t => t.TokenTypeIds).ToArray(),
                    };
                    // Create input tensor.

                    var input_ids = ConvertToTensor(bertInput.InputIds, bertInput.InputIds.Length);
                    var attention_mask = ConvertToTensor(bertInput.AttentionMask, bertInput.InputIds.Length);
                    var token_type_ids = ConvertToTensor(bertInput.TypeIds, bertInput.InputIds.Length);


                    // Create input data for session.
                    var input = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", input_ids),
                        NamedOnnxValue.CreateFromTensor("input_mask", attention_mask),
                        NamedOnnxValue.CreateFromTensor("segment_ids", token_type_ids) };

                    // Create an InferenceSession from the Model Path.
                    IReadOnlyList<NamedOnnxValue>? output;
                    lock (session)
                    {
                        // Run session and send the input data in to get inference output. 
                        output = session.Run(input);
                    }
                    cancelToken.ThrowIfCancellationRequested();

                    // Call ToList on the output.
                    // Get the First and Last item in the list.
                    // Get the Value of the item and cast as IEnumerable<float> to get a list result.
                    List<float> startLogits = (output.ToList().First().Value as IEnumerable<float>).ToList();
                    List<float> endLogits = (output.ToList().Last().Value as IEnumerable<float>).ToList();

                    // Get the Index of the Max value from the output lists.
                    var startIndex = startLogits.ToList().IndexOf(startLogits.Max());
                    var endIndex = endLogits.ToList().IndexOf(endLogits.Max());

                    // From the list of the original tokens in the sentence
                    // Get the tokens between the startIndex and endIndex and convert to the vocabulary from the ID of the token.
                    var predictedTokens = tokens
                                .Skip(startIndex)
                                .Take(endIndex + 1 - startIndex)
                                .Select(o => tokenizer.IdToToken((int)o.VocabularyIndex))
                                .ToList();

                    // Print the result.
                    //Console.WriteLine(String.Join(" ", predictedTokens));
                    var answer = String.Join(" ", predictedTokens);
                    cancel.ThrowIfCancellationRequested();
                    return answer;
                }
                catch (Exception ex) { throw new Exception($"AnswerBert: {ex}"); }
            }, cancel, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public static Tensor<long> ConvertToTensor(long[] inputArray, int inputDimension)
        {
            // Create a tensor with the shape the model is expecting. Here we are sending in 1 batch with the inputDimension as the amount of tokens.
            Tensor<long> input = new DenseTensor<long>(new[] { 1, inputDimension });

            // Loop through the inputArray (InputIds, AttentionMask and TypeIds)
            for (var i = 0; i < inputArray.Length; i++)
            {
                // Add each to the input Tenor result.
                // Set index and array value of each input Tensor.
                input[0, i] = inputArray[i];
            }
            return input;
        }

        public class BertInput
        {
            public long[] InputIds { get; set; }
            public long[] AttentionMask { get; set; }
            public long[] TypeIds { get; set; }
        }


    }
}