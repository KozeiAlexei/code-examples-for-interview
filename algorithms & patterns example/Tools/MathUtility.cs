using System;

namespace VoiceRecognizeMark.Tools
{
    public static class MathUtility
    {
        public static double ToMell(double f) => 1127.0 * Math.Log(1.0 + f / 700.0);

        public static double FromMell(double m) => 700 * (Math.Exp(m / 1127) - 1);
    }
}
