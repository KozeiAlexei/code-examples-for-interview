using System;

namespace VoiceRecognizeMark.Model
{
    public class Word
    {
        public Word(int id)
        {
            Id = id;
        }

        public Word(int id, Frame[] frames)
        {
            Id = id;
            Frames = frames;
        }

        public int Id { get; set; }

        public Frame[] Frames { get; set; }
    }
}
