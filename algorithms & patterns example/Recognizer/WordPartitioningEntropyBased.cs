using System;
using System.Linq;
using System.Collections.Generic;

using VoiceRecognizeMark.Model;
using VoiceRecognizeMark.Algorithms;
using VoiceRecognizeMark.Model.Recognizer;
using VoiceRecognizeMark.Abstract.Recognizer;

namespace VoiceRecognizeMark.Recognizer
{
    public class WordPartitioningEntropyBased : IPartitioner<Word, Frame[]>
    {
        #region Private models

        private class SilenceAvailabilityModel
        {
            public bool HasSilence { get; set; }

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

        private WordPartitioningParamsModel partitioningParams { get; set; }

        public WordPartitioningEntropyBased(WordPartitioningParamsModel partitioniongParams)
        {
            this.partitioningParams = partitioniongParams;
        }

        public Word[] Partitioning(Frame[] signalFrames)
        {
            var words = new List<Word>();

            var silenceAvailability = FindSilenceThreshold(signalFrames);

            int wordId = -1;
            int firstWordFrame = -1;

            var wordBoundaries = new Dictionary<int, Boundary>();

            if (silenceAvailability.HasSilence)
            {
                var lastWord = default(Word);
                foreach (var frame in signalFrames)
                {
                    if (EntropyHelper.RMS(frame.Signal) > silenceAvailability.SilenceThreshold)
                    {
                        // Шум

                        if (firstWordFrame == -1)
                            firstWordFrame = frame.Id;
                    }
                    else
                    {
                        // Голос

                        if (firstWordFrame >= 0)
                        {
                            var distanceBetweenWords = 0;
                            if (lastWord != null)
                                distanceBetweenWords = firstWordFrame - wordBoundaries[lastWord.Id].Right;

                            //Новое слово
                            if (lastWord == null || distanceBetweenWords > partitioningParams.MinDistanceBetweenWords)
                            {
                                wordId++;
                                lastWord = new Word(wordId);

                                wordBoundaries.Add(lastWord.Id, new Boundary(firstWordFrame, frame.Id));
                                words.Add(lastWord);
                            }

                            //Нужно добавить к предыдущему
                            else if (lastWord != null && distanceBetweenWords < partitioningParams.MinDistanceBetweenWords)
                            {
                                var currentWordRms = 0.0;
                                for (int i = firstWordFrame; i < frame.Id; i++)
                                    currentWordRms += EntropyHelper.RMS(signalFrames[i].Signal);

                                currentWordRms /= (frame.Id - firstWordFrame);

                                // Добавляем, если имеет значимый RMS
                                if (currentWordRms > silenceAvailability.SilenceThreshold * 2)
                                    wordBoundaries[lastWord.Id].Right = frame.Id;
                            }

                            firstWordFrame = -1;
                        }
                    }
                }
            }
            //else
            //    return new Word[] { new Word(0) { Frames = signalFrames } };

            // Оптимизация: возможно стоит вырезать на ходу
            var bound = default(Boundary);
            foreach (var word in words)
            {
                bound = wordBoundaries[wordId];
                word.Frames = CutFromArray(bound.Left, bound.Right, signalFrames);
            }

            return words.ToArray();
        }

        private TObject[] CutFromArray<TObject>(int start, int end, IEnumerable<TObject> array)
        {
            var collection = new TObject[end - start + 1];
            for (int i = start; i <= end; i++)
                collection[i - start] = array.ElementAt(i);

            return collection;
        }

        private SilenceAvailabilityModel FindSilenceThreshold(Frame[] signalFrames)
        {
            var rms = default(double);
            var rmsMax = default(double);
            var rmsSilence = default(double);

            rms = rmsMax = rmsSilence;

            var hasSilence = false;


            var silenceFrameCount = 0;
            foreach (var frame in signalFrames)
            {
                rms = EntropyHelper.RMS(frame.Signal);
                rmsMax = Math.Max(rmsMax, rms);

                var entropy = EntropyHelper.Entropy(frame.Signal, partitioningParams.EntropyBins, -1, 1);
                if (entropy > partitioningParams.EntropyThreshold)
                {
                    hasSilence = true;
                    rmsSilence += EntropyHelper.RMS(frame.Signal);

                    silenceFrameCount++;
                }
            }

            rmsSilence /= silenceFrameCount;

            return new SilenceAvailabilityModel()
            {
                HasSilence = hasSilence,
                SilenceThreshold = rmsSilence * 2
            };
        }
    }
}
