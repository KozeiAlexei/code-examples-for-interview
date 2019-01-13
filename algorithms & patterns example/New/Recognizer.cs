using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceRecognizeMark.Abstract.Learning;
using VoiceRecognizeMark.Abstract.Recognizer;
using VoiceRecognizeMark.Algorithms;
using VoiceRecognizeMark.Model.Recognizer;

namespace VoiceRecognizeMark.New
{
    public class CredibilityEstimationResultNew
    {
        public string Word { get; set; }

        public double Mark { get; set; }
    }

    public class CredibilityEstimationNew
    {
        private class BestModelData
        {
            public string Word { get; set; }

            public string Accent { get; set; }

            public MFCCWord WordAdditionalData { get; set; }
        }

        private class BestWordModel
        {
            public double Distance { get; set; }

            public BestModelData AdditionalData { get; set; }
        }

        private IDigitalTimeWarping DTWAlgorithm { get; set; }

        private IEnumerable<BestModelData> LearingModel { get; set; }

        public CredibilityEstimationNew(
            Dictionary<string, Dictionary<int, Dictionary<string, List<WordWithAccent>>>> learingDatabase)
        {
            var learningModel = new List<BestModelData>();
            foreach (var languageEntry in learingDatabase)
            {
                foreach (var speakerEntry in languageEntry.Value)
                {
                    foreach (var wordEntry in speakerEntry.Value)
                    {
                        foreach (var accentEntry in wordEntry.Value)
                            learningModel.Add(new BestModelData() { Accent = accentEntry.AccentName, Word = wordEntry.Key, WordAdditionalData = accentEntry.MFCC });
                    }
                }
            }

            LearingModel = learningModel.AsEnumerable();
        }

        private double MarkFunction(double distance)
        {
            ////if (distance < 300)
            ////    return 1.0;

            ////if (distance > 800)
            ////    return 0.0;

            ////return Math.Exp(-distance / 300);

            //var m = (1000 - distance) / 1000;

            //if (m < 0.0)
            //    m = 0.0;
            //if (m > 1.0)
            //    m = 1.0;

            //return m;
            return distance;
        }

        public CredibilityEstimationResultNew Estimate(MFCCWord word)
        {
            var bestModel = FindBestModel(word);

            //var marks = new double[bestModels.Count];
            //for (int i = 0; i < marks.Length; i++)
            //    //marks[i] = (1000 - bestModels[i].Distance) / 1000; //Над знаменателем ещё стоит подумать
            //var average = marks.Sum() / (double)marks.Count();

            var mark = MarkFunction(bestModel.Distance);

            return new CredibilityEstimationResultNew()
            {
                Mark = mark,
                Word = bestModel.AdditionalData.Word
            };
        }

        private BestWordModel FindBestModel(MFCCWord mfccWord)
        {
            var bestModel = default(BestModelData);

            var distanceMin = double.MaxValue;
            var currentMFCC = mfccWord.MFCC.SelectMany(c => c).ToArray();
            foreach (var model in LearingModel.Where(w => w.Word == "a"))
            {
                var dtw = new DTW();

                var distance = dtw.CalcDistanceMFCC(
                    mfccWord.MFCC.AsEnumerable(), 
                    model.WordAdditionalData.MFCC.AsEnumerable() 
                );

                //var distance = DTWAlgorithm.CalcDistance(
                //    model.WordAdditionalData.MFCC.SelectMany(c => c).ToArray(),
                //    currentMFCC
                //);


                //var distance = 0.0;
                //foreach (var mfccEntry in model.WordAdditionalData.MFCC)
                //    distance += DTWAlgorithm.CalcDistanceVector(currentMFCC, mfccEntry, mfccEntry.Length);

                //distance /= model.WordAdditionalData.MFCC.Count();

                if (bestModel == null || distance < distanceMin)
                {
                    bestModel = model;
                    distanceMin = distance;
                }
            }

            return new BestWordModel()
            {
                Distance = distanceMin,
                AdditionalData = bestModel
            };
        }
    }
}
