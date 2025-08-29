using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Drawing;
using System.Net;

namespace Heming
{
    public class Predicting : IPredicting
    {
        public InferenceSession Session { get; set; }
        public Predicting()
        {

        }
        public Predicting(InferenceSession session)
        {
            Session = session;
        }

        public IList<PredictionResult> RemotePredicting(string imgpath, string url)
        {
            WebClient webClient = new WebClient();
            byte[] res = webClient.UploadFile(url, imgpath);
            string resjson = System.Text.Encoding.ASCII.GetString(res);
            return JsonConvert.DeserializeObject<IList<PredictionResult>>(resjson);
        }

        public IList<PredictionResult> LocalPredicting(Stream imgstream)
        {
            SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(imgstream);
            return CalculatePrediction(image);
        }

        public IList<PredictionResult> LocalPredicting(string imgpath)
        {
            SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(imgpath);
            return CalculatePrediction(image);
        }

        IList<PredictionResult> CalculatePrediction(SixLabors.ImageSharp.Image image, int imageSize = 320)
        {
            MemoryStream memory = new MemoryStream();
            var input = new DenseTensor<float>(new[] { 1, 3, imageSize, imageSize });
            image.Mutate(x => x.Resize(imageSize, imageSize));
            image.SaveAsBmp(memory);
            Bitmap bmp = new Bitmap(memory);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    input[0, 0, y, x] = bmp.GetPixel(x, y).R;
                    input[0, 1, y, x] = bmp.GetPixel(x, y).G;
                    input[0, 2, y, x] = bmp.GetPixel(x, y).B;
                }
            }
            var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor<float>("image_tensor", input)
                };
            var resultsCollection = Session.Run(inputs);
            var resultsDict = resultsCollection.ToDictionary(x => x.Name, x => x);
            var detectedBoxes = resultsDict["detected_boxes"].AsTensor<float>();
            var detectedClasses = resultsDict["detected_classes"].AsTensor<long>();
            var detectedScores = resultsDict["detected_scores"].AsTensor<float>();

            var numBoxes = detectedClasses.Length;
            List<PredictionResult> predictions = new List<PredictionResult>();
            for (int i = 0; i < numBoxes; i++)
            {
                predictions.Add(new PredictionResult()
                {
                    Probability = detectedScores[0, i],
                    BoundingBox = new BoundingBox()
                    {
                        Left = detectedBoxes[0, i, 0],
                        Top = detectedBoxes[0, i, 1],
                        Width = detectedBoxes[0, i, 2] - detectedBoxes[0, i, 0],
                        Height = detectedBoxes[0, i, 3] - detectedBoxes[0, i, 1]
                    }
                });
            }
            return predictions;
        }
    }
}
