using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceRecognizeMark.Abstract.Recognizer;
using VoiceRecognizeMark.Algorithms;
using VoiceRecognizeMark.Model;
using VoiceRecognizeMark.Model.Algorithms;
using VoiceRecognizeMark.Model.Recognizer;
using VoiceRecognizeMark.Recognizer;

namespace VoiceRecognizeMark.SimpleRecognizer
{
    public class SimpleMFCCBuilder
    {
        public static IMFCCBuilder<Word, MFCCWord> Create(int samplesPerSecond)
        {
            var mfccBuilderParams = new MFCCBuilderParamsModel()
            {
                MFCCCount = 12,
                MFCCFrequencyMax = 4000,
                MFCCFrequencyMin = 300,
                SamplesPerSecond = samplesPerSecond
            };

            return new MFCCBuilder(
                builderParams: mfccBuilderParams,

                dctAlgorithm: new DCT(),
                fftAlgorithm: new FFT(),
                windowAlgorithm: new HammingWindow(),
                preEmphasisAlgorithm: new PreEmphasis(),
                mellFilterAlgorithm: new MellFilterGenerator(),
                logarithmPowerAlogorithm: new LogarithmPower()
            );
        }
    }
}
