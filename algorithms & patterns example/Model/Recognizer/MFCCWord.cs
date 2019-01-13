using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace VoiceRecognizeMark.Model.Recognizer
{
    [Serializable]
    public class MFCCWord
    {
        public MFCCWord(Word word, List<double[]> mfcc)
        {
            Word = word;
            MFCC = mfcc;
        }

        [NonSerialized]
        public Word Word;

        public List<double[]> MFCC { get; set; }
    }
}
