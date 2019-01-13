using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using NAudio.Wave;

using VoiceRecognizeMark.Model;
using VoiceRecognizeMark.Recognizer;
using VoiceRecognizeMark.Model.Recognizer;
using VoiceRecognizeMark.SimpleRecognizer;
using VoiceRecognizeMark.Abstract.Learning;
using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using VoiceRecognizeMark.NoiseReduction;
using System.Linq;

namespace VoiceRecognizeMark.New
{
    public class SystemLearningNew : ISystemLearning
    {
        private JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented
        };

        public void CreateLearningDatabase(string inputPath, string outputFileName)
        {
            var dictionary = CreateLearningDictionary(inputPath);

            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outputFileName), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                formatter.Serialize(stream, dictionary);
                stream.Close();
            }
        }

        public Dictionary<string, Dictionary<int, Dictionary<string, List<WordWithAccent>>>> LoadLearingDatabase(string path)
        {
            var dictionary = default(Dictionary<string, Dictionary<int, Dictionary<string, List<WordWithAccent>>>>);

            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path), FileMode.Open, FileAccess.Read, FileShare.None))
            {
                dictionary = formatter.Deserialize(stream) as
                    Dictionary<string, Dictionary<int, Dictionary<string, List<WordWithAccent>>>>;
            }

            return dictionary;
        }

        private Dictionary<string, Dictionary<int, Dictionary<string, List<WordWithAccent>>>> CreateLearningDictionary(string path)
        {
            var dictionary = new Dictionary<string, Dictionary<int, Dictionary<string, List<WordWithAccent>>>>();

            foreach (var languageDir in new DirectoryInfo(path).EnumerateDirectories())
            {
                var languageKey = languageDir.Name;

                var frameParams = new FrameParams()
                {
                    FrameLength = 47,
                    FrameOverlap = 0.5
                };

                dictionary.Add(languageKey, new Dictionary<int, Dictionary<string, List<WordWithAccent>>>());
                foreach(var speakerDir in languageDir.EnumerateDirectories())
                {
                    var speakerKey = int.Parse(speakerDir.Name); ;

                    dictionary[languageKey].Add(speakerKey, new Dictionary<string, List<WordWithAccent>>());
                    foreach(var wordDir in speakerDir.EnumerateDirectories())
                    {
                        var wordKey = wordDir.Name;

                        dictionary[languageKey][speakerKey].Add(wordKey, new List<WordWithAccent>());
                        foreach(var accentFile in wordDir.EnumerateFiles())
                        {
                            var signalFormat = default(WaveFormat);

                            var signal = ReadAudioData(accentFile.FullName, out signalFormat);

                            var mfccBuilder = new MFCC(signalFormat.SampleRate);

                            var withoutNoiseSignla = new ThresholdNoiseReduction(1).Process(signal.Select(a => a * 32767.0).ToArray()).SkipWhile(a => a == 0).ToArray();


                            dictionary[languageKey][speakerKey][wordKey].Add(new WordWithAccent()
                            {
                                AccentName = accentFile.Name,
                                MFCC = new MFCCWord(null, mfccBuilder.Process(withoutNoiseSignla).ToList())
                            });
                        }
                    }
                }
            }

            return dictionary;
        }

        private double[] ReadAudioData(string fileName, out WaveFormat signalFormat)
        {
            var signal = default(double[]);
            using (var sampleFile = new WaveFileReader(fileName))
            {
                var sampleSignalBytesPerSample = sampleFile.WaveFormat.BitsPerSample / 8;

                signal = new double[sampleFile.Length / sampleSignalBytesPerSample];
                for (int index = 0; index < signal.Length; index++)
                    signal[index] = sampleFile.ReadNextSampleFrame()[0];

                signalFormat = sampleFile.WaveFormat;
            }

            return signal;
        }
    }
}
