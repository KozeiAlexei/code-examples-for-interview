using System;
using System.Linq;
using System.Collections.Generic;

using VoiceRecognizeMark.Model;
using VoiceRecognizeMark.Algorithms;
using VoiceRecognizeMark.Model.Recognizer;
using VoiceRecognizeMark.Abstract.Recognizer;

namespace VoiceRecognizeMark.Recognizer
{
    public class WordPartitioningRMSBasedParams
    {
        public int MinFrameDistanceBeetwenWords { get; set; }

        public double RMSIndeterminacy { get; set; }
    }

    public class WordPartitioningRMSBased : IPartitioner<Word, Frame[]>
    {
        #region Private models

        private class VoiceAvailability
        {
            public bool HasVoice { get; set; }

            public double SilenceThreshold { get; set; }
        }

        private class Boundary
        {
            public Boundary(int left, int right)
            {
                Left = left;
                Right = right;
            }

            public int Left { get; set; }

            public int Right { get; set; }
        }

        #endregion

        private WordPartitioningRMSBasedParams partitioningParams { get; set; }

        public WordPartitioningRMSBased(WordPartitioningRMSBasedParams partitioniongParams)
        {
            this.partitioningParams = partitioniongParams;
        }

        public Word[] Partitioning(Frame[] signalFrames)
        {
            var words = new List<Word>();

            var silenceAvailability = FindSilenceThreshold(signalFrames);

            if (silenceAvailability.HasVoice)
            {
                var currentWordId = 0;
                var currentWordFrames = new List<Frame>();
                
                // FOR DEBUG
                //var framesRms = new double[signalFrames.Length];
                //for (int i = 0; i < framesRms.Length; i++)
                //    framesRms[i] = EntropyHelper.RMS(signalFrames[i].Signal);

                var currentFrame = default(Frame);
                var currentLastTakedFrame = default(Frame);

                for (int i = 0; i < signalFrames.Length; i++)
                {
                    currentFrame = signalFrames[i];

                    if (EntropyHelper.RMS(currentFrame.Signal) > silenceAvailability.SilenceThreshold)
                    {
                        if (currentLastTakedFrame == null)
                            currentWordFrames.Add(currentFrame);
                        else
                        {
                            //Обработка ситуации проседания сигнала при произношении гласных
                            if (currentFrame.Id - currentLastTakedFrame.Id < partitioningParams.MinFrameDistanceBeetwenWords)
                            {
                                for (int j = currentLastTakedFrame.Id + 1; j <= currentFrame.Id; j++)
                                    currentWordFrames.Add(signalFrames[j]);
                            }
                            else
                            {
                                words.Add(new Word(currentWordId++, currentWordFrames.ToArray()));

                                currentWordFrames.Clear();
                                currentWordFrames.Add(currentFrame);
                            }
                        }

                        currentLastTakedFrame = currentFrame;
                    }
                }

                if(currentWordFrames.Any())
                    words.Add(new Word(currentWordId++, currentWordFrames.ToArray()));
            }
            

            return words.ToArray();
        }

        private VoiceAvailability FindSilenceThreshold(Frame[] signalFrames)
        {
            var hasVoice = false;

            var framesRMS = new double[signalFrames.Length];
            for (int i = 0; i < framesRMS.Length; i++)
                framesRMS[i] = EntropyHelper.RMS(signalFrames[i].Signal);

            var cleanedFrames = framesRMS.Where(rms => rms != 0);

            var rmsAverage = default(double);
            if (cleanedFrames.Any())
            {
                hasVoice = true;
                rmsAverage = (cleanedFrames.Sum() / cleanedFrames.Count()) * partitioningParams.RMSIndeterminacy;
            }

            return new VoiceAvailability()
            {
                HasVoice = hasVoice,
                SilenceThreshold = rmsAverage
            };
        }
    }
}
