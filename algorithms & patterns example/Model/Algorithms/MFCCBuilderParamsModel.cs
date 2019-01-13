using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceRecognizeMark.Model.Algorithms
{
    public class MFCCBuilderParamsModel
    {
        public int MFCCCount { get; set; }

        public int MFCCFrequencyMin { get; set; }

        public int MFCCFrequencyMax { get; set; }

        public int SamplesPerSecond { get; set; }
    }
}
