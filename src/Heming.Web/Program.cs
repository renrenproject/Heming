global using Microsoft.ML.OnnxRuntime;
using Heming;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IPredicting, Predicting>(p => new Predicting(new InferenceSession("HemingModel.onnx")));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
