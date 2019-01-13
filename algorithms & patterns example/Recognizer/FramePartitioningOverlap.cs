using System.Linq;
using System.Collections.Generic;

using VoiceRecognizeMark.Model;
using VoiceRecognizeMark.Model.Recognizer;
using VoiceRecognizeMark.Abstract.Recognizer;
using System;

namespace VoiceRecognizeMark.Recognizer
{ 
    public class FramePartitioningOverlap : IPartitioner<Frame, double[]>
    {
        private FrameParams frameParams = default(FrameParams);
        private SignalParams signalParams = default(SignalParams);

        public FramePartitioningOverlap(SignalParams signalParams, FrameParams frameParams)
        {
            this.frameParams = frameParams;
            this.signalParams = signalParams;
        }

        public Frame[] Partitioning(double[] signal)
        {
            var bytesPerFrame = (int)Math.Pow(2, Math.Floor(Math.Log((int)(signalParams.BytesPerSecond * frameParams.FrameLength / 1000.0), 2)));
            var bytesPerSample = signalParams.BitsPerSample / 8;

            var samplePerFrame = bytesPerFrame / bytesPerSample;
            var samplePerFrameNotOverlap = (int)(samplePerFrame * (1 - frameParams.FrameOverlap));

            var framesCount = (int)((signalParams.Subchunk2Size / bytesPerSample) / samplePerFrameNotOverlap);

            var frames = new List<Frame>(framesCount);
            for(int i = 0; i < framesCount; i++)
            {
                frames.Add(new Frame()
                {
                    Id = i,
                    Signal = signal.Skip(i * samplePerFrameNotOverlap).Take(samplePerFrame).ToArray()
                });
            }

            return frames.ToArray();
        }
    }
}
