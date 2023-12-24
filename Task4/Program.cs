using Task4;
using NuGetBertSpace;
var builder = WebApplication.CreateBuilder(args);

CancellationTokenSource cts = new CancellationTokenSource();
string modelPath = "C:\\Users\\mt27\\Downloads\\bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
NuGetBertSpace.NuGetBert answerTask;
answerTask = new NuGetBertSpace.NuGetBert(modelPath, cts.Token);
_ = answerTask.Download();

builder.Services.AddControllers();
builder.Services.AddSingleton<NuGetBert>(answerTask);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(builder =>
{
    builder
        .WithOrigins("*")
        .WithHeaders("*")
        .WithMethods("*");
});

app.MapControllers();
app.Run();

public partial class Program
{

}