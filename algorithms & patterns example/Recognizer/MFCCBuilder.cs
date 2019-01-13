using System.Linq;
using System.Collections.Generic;

using VoiceRecognizeMark.Model;
using VoiceRecognizeMark.Abstract;
using VoiceRecognizeMark.Model.Algorithms;
using VoiceRecognizeMark.Model.Recognizer;
using VoiceRecognizeMark.Abstract.Recognizer;

namespace VoiceRecognizeMark.Recognizer
{
    public class MFCCBuilder : IMFCCBuilder<Word, MFCCWord>
    {
        private MFCCBuilderParamsModel BuilderParams { get; set; }

        private IAlgorithm<double[], double[]> PreEmphasisArgorithm { get; set; }

        private IAlgorithm<double[], double[]> FFTAlgorithm { get; set; }
        private IAlgorithm<double[], double[]> DCTAlgorithm { get; set; }

        private IAlgorithm<double[], double[]> WindowAlgorithm { get; set; }
        private IAlgorithm<LogarithmPowerParamsModel, double[]> LogarithmPowerAlogorithm { get; set; }
        private IAlgorithm<MellFilterGeneratorParamsModel, double[,]> MellFilterAlgorithm { get; set; }

        public MFCCBuilder(
            MFCCBuilderParamsModel builderParams,
            IAlgorithm<double[], double[]> dctAlgorithm,
            IAlgorithm<double[], double[]> fftAlgorithm,
            IAlgorithm<double[], double[]> windowAlgorithm,
            IAlgorithm<double[], double[]> preEmphasisAlgorithm,
            IAlgorithm<LogarithmPowerParamsModel, double[]> logarithmPowerAlogorithm,
            IAlgorithm<MellFilterGeneratorParamsModel, double[,]> mellFilterAlgorithm)
        {
            BuilderParams = builderParams;

            FFTAlgorithm = fftAlgorithm;
            DCTAlgorithm = dctAlgorithm;
            WindowAlgorithm = windowAlgorithm;
            MellFilterAlgorithm = mellFilterAlgorithm;
            PreEmphasisArgorithm = preEmphasisAlgorithm;
            LogarithmPowerAlogorithm = logarithmPowerAlogorithm;
        }

        public MFCCWord Build(Word word)
        {
            // 1. Акцентирование(избавление от шумов)
            var preEmphasisProcessedWord = new Word(word.Id, new Frame[word.Frames.Length]);
            for (int i = 0; i < word.Frames.Length; i++)
            {
                preEmphasisProcessedWord.Frames[i] = new Frame(
                    id: word.Frames[i].Id,
                    signal: PreEmphasisArgorithm.Execute(word.Frames[i].Signal)
                );
            }

            preEmphasisProcessedWord = word;

            // 2. Применяем оконную функцию, для сглаживания краев фрейма.
            //    Делается это исходя из того, что преобразование Фурье выполняется для бесконечно повторяющегося сигнала

            var windowProcessedWord = new Word(preEmphasisProcessedWord.Id, new Frame[preEmphasisProcessedWord.Frames.Length]);
            for(int i = 0; i < preEmphasisProcessedWord.Frames.Length; i++)
            {
                windowProcessedWord.Frames[i] = new Frame(
                    id: preEmphasisProcessedWord.Frames[i].Id,
                    signal: WindowAlgorithm.Execute(preEmphasisProcessedWord.Frames[i].Signal)
                );
            }

            // 3. Выполнение преобразования Фурье для получения частотного спектра сигнала

            var fftProcessedWord = new Word(windowProcessedWord.Id, new Frame[windowProcessedWord.Frames.Length]);
            for(int i = 0; i < windowProcessedWord.Frames.Length; i++)
            {
                fftProcessedWord.Frames[i] = new Frame(
                    id: windowProcessedWord.Frames[i].Id,
                    signal: FFTAlgorithm.Execute(windowProcessedWord.Frames[i].Signal)
                );
            }

            // 4. Построение мелл-фильтров

            var mellFilters = MellFilterAlgorithm.Execute(new MellFilterGeneratorParamsModel()
            {
                Frequency = BuilderParams.SamplesPerSecond,
                FrequencyMin = BuilderParams.MFCCFrequencyMin,
                FrequencyMax = BuilderParams.MFCCFrequencyMax,

                MFCCCount = BuilderParams.MFCCCount,
                FilterLength = fftProcessedWord.Frames.First().Signal.Length
            });

            // 5. Логарифмирование энергии спектра

            var logPowerProcessedWord = new Word(fftProcessedWord.Id, new Frame[fftProcessedWord.Frames.Length]);
            for(int i = 0; i < fftProcessedWord.Frames.Length; i++)
            {
                logPowerProcessedWord.Frames[i] = new Frame(
                    id: fftProcessedWord.Frames[i].Id,
                    signal: LogarithmPowerAlogorithm.Execute(new LogarithmPowerParamsModel()
                    {
                        Signal = fftProcessedWord.Frames[i].Signal,
                        MFCCCount = BuilderParams.MFCCCount, MellFilters = mellFilters
                    })
                );
            }

            // 6. Применение дискретного косинусного преобразования

            var mfccWord = new MFCCWord(logPowerProcessedWord, new List<double[]>(logPowerProcessedWord.Frames.Length));
            foreach (var frame in logPowerProcessedWord.Frames)
                mfccWord.MFCC.Add(DCTAlgorithm.Execute(frame.Signal.Take(BuilderParams.MFCCCount).ToArray()));

            return mfccWord;
        }
    }
}
