using System.Threading.Tasks;
using System.Collections.Concurrent;

using VoiceRecognizeMark.Model;
using VoiceRecognizeMark.Model.Recognizer;
using VoiceRecognizeMark.Abstract.Recognizer;
using System.Collections.Generic;
using System.Linq;

namespace VoiceRecognizeMark.Recognizer
{
    public class RecognizerMark
    {
        private IMFCCBuilder<Word, MFCCWord> MFCCBuilder { get; set; }
        private IPartitioner<Word, Frame[]> WordPartitioner { get; set; }
        private IPartitioner<Frame, double[]> FramePartitioner { get; set; }
        private ICredibilityEstimation<CredibilityEstimationResult, MFCCWord[]> CredibilityEstimation { get; set; }

        public RecognizerMark(
            IMFCCBuilder<Word, MFCCWord> mfccBuilder,
            IPartitioner<Word, Frame[]> wordPartitioner,
            IPartitioner<Frame, double[]> framePartitioner,
            ICredibilityEstimation<CredibilityEstimationResult, MFCCWord[]> credibilityEstimation)
        {
            MFCCBuilder = mfccBuilder;
            WordPartitioner = wordPartitioner;
            FramePartitioner = framePartitioner;
            CredibilityEstimation = credibilityEstimation;
        }

        public CredibilityEstimationResult Evaluate(double[] signal)
        {
            //1. Разбиение на фреймы
            var frames = FramePartitioner.Partitioning(signal);

            //2. Разбиение на слова
            var words = WordPartitioner.Partitioning(frames);

            var tmp = new List<Word> { new Word(1, words.SelectMany(w => w.Frames).ToArray()) };

            //3. Получение MFCC коэффициентов для слов
            var mfccWords = new ConcurrentBag<MFCCWord>();
            Parallel.ForEach(tmp, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, (word) => mfccWords.Add(MFCCBuilder.Build(word)));

            //4. Сопоставление коэффициентов и оценка правдоподобия
            return CredibilityEstimation.Estimate(mfccWords.ToArray());
        }
    }
}
