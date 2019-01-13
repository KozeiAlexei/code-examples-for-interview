using System;

namespace VoiceRecognizeMark.Model
{
    [Serializable]
    public class Frame
    {
        public Frame() { }

        public Frame(int id, double[] signal)
        {
            Id = id;
            Signal = signal;
        }

        public int Id { get; set; }

        public double[] Signal { get; set; }
    }
}
