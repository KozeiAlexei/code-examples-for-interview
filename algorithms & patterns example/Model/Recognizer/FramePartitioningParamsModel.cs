using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceRecognizeMark.Model.Recognizer
{
    public class FrameParams
    {
        public int FrameLength { get; set; }

        public double FrameOverlap { get; set; }
    }

    public class SignalParams
    {
        public int BitsPerSample { get; set; }

        public int BytesPerSecond { get; set; }

        public long Subchunk2Size { get; set; }
    }
}
