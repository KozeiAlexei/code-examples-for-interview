using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceRecognizeMark.Model.Recognizer;

namespace VoiceRecognizeMark.Abstract.Learning
{
    [Serializable]
    public class WordWithAccent
    {
        public string AccentName { get; set; }

        public MFCCWord MFCC { get; set; }
    }

    public interface ISystemLearning
    {
        void CreateLearningDatabase(string inputPath, string outputFileName);

        Dictionary<string, Dictionary<int, Dictionary<string, List<WordWithAccent>>>> LoadLearingDatabase(string path);
    }
}
