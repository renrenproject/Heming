using Microsoft.ML.OnnxRuntime;

namespace Heming
{
    public interface IPredicting
    {
        public InferenceSession Session
        {
            get; set;
        }

        public IList<PredictionResult> LocalPredicting(string imgpath);
        public IList<PredictionResult> LocalPredicting(Stream imgstream);
    }
}
