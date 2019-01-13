using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceRecognizeMark.Model.Recognizer
{
    public class WordPartitioningParamsModel
    {
        public int EntropyBins { get; set; }
        public double EntropyThreshold { get; set; }

        public double MinDistanceBetweenWords { get; set; }
    }
}
