using System;
using System.Collections.Generic;
using System.Linq;
using VoiceRecognizeMark.Abstract.Recognizer;

namespace VoiceRecognizeMark.Algorithms
{
    public class DTW : IDigitalTimeWarping
    {
        public double CalcDistance(double[] vectorA, double[] vectorB)
        {
            var matrixDiff = new double[vectorA.Length, vectorB.Length];
            for(int i = 0; i < vectorA.Length; i++)
            {
                for (int j = 0; j < vectorB.Length; j++)
                    matrixDiff[i, j] = Math.Abs(vectorA[i] - vectorB[j]);
            }

            return FindDistance(matrixDiff);
        }

        public double CalcDistanceVector(double[] vectorA, double[] vectorB, int resultVectorSize)
        {
            var vectorASizeV = vectorA.Length / resultVectorSize;
            var vectorBSizeV = vectorB.Length / resultVectorSize;

            var matrixDiff = new double[vectorASizeV, vectorBSizeV];

            for(int i = 0; i < vectorASizeV; i++)
            {
                for(int j = 0; j < vectorBSizeV; j++)
                {
                    var distance = 0.0;
                    for (int k = 0; k < resultVectorSize; k++)
                        distance += Math.Pow(vectorA[i * resultVectorSize + k] - vectorB[j * resultVectorSize + k], 2);

                    matrixDiff[i, j] = Math.Sqrt(distance);
                }
            }

            return FindDistance(matrixDiff);
        }

        private double EuclidDistance(double[] vectorA, double[] vectorB)
        {
            var squaredDistance = 0.0;
            for (int i = 0; i < vectorA.Length; i++)
                squaredDistance += Math.Pow(vectorA[i] - vectorB[i], 2.0);

            return Math.Sqrt(squaredDistance);
        }

        public double CalcDistanceMFCC(IEnumerable<double[]> mfccA, IEnumerable<double[]> mfccB)
        {
            var matrixDiff = new double[mfccA.Count(), mfccB.Count()];
            for(int i = 0; i < mfccA.Count(); i++)
            {
                for (int j = 0; j < mfccB.Count(); j++)
                    matrixDiff[i, j] = EuclidDistance(mfccA.ElementAt(i), mfccB.ElementAt(j));
            }

            return FindDistance(matrixDiff);
        }

        private double FindDistance(double[,] matrixDiff)
        {
            var vectorASize = matrixDiff.GetLength(0);
            var vectorBSize = matrixDiff.GetLength(1);

            var matrixPath = new double[vectorASize, vectorBSize];

            matrixPath[0, 0] = matrixDiff[0, 0];

            for (int i = 1; i < vectorASize; i++)
                matrixPath[i, 0] = matrixDiff[i, 0] + matrixPath[i - 1, 0];

            for (int j = 1; j < vectorBSize; j++)
                matrixPath[0, j] = matrixDiff[0, j] + matrixPath[0, j - 1];

            for (int i = 1; i < vectorASize; i++)
            {
                for (int j = 1; j < vectorBSize; j++)
                {
                    if (matrixPath[i - 1, j - 1] < matrixPath[i - 1, j])
                    {
                        if (matrixPath[i - 1, j - 1] < matrixPath[i, j - 1])
                            matrixPath[i, j] = matrixDiff[i, j] + matrixPath[i - 1, j - 1];
                        else
                            matrixPath[i, j] = matrixDiff[i, j] + matrixPath[i, j - 1];
                    }
                    else
                    {
                        if (matrixPath[i - 1, j] < matrixPath[i, j - 1])
                            matrixPath[i, j] = matrixDiff[i, j] + matrixPath[i - 1, j];
                        else
                            matrixPath[i, j] = matrixDiff[i, j] + matrixPath[i, j - 1];
                    }
                }
            }

            var backwardResult = BackwardDirection(matrixPath);

            return backwardResult.Sum() / backwardResult.Count();
        }

        private double[] BackwardDirection(double [,] matrixPath)
        {
            var vectorASize = matrixPath.GetLength(0);
            var vectorBSize = matrixPath.GetLength(1);

            var warpPath = new double[vectorASize * vectorBSize];
            var warpPathIndex = 0;

            var i = vectorASize - 1;
            var j = vectorBSize - 1;

            warpPath[warpPathIndex] = matrixPath[i, j];

            do
            {
                if (i > 0 && j > 0)
                {
                    if (matrixPath[i - 1, j - 1] < matrixPath[i - 1, j])
                    {
                        if (matrixPath[i - 1, j - 1] < matrixPath[i, j - 1])
                        {
                            i--;
                            j--;
                        }
                        else
                            j--;
                    }
                    else
                    {
                        if (matrixPath[i - 1, j] < matrixPath[i, j - 1])
                            i--;
                        else
                            j--;
                    }
                }
                else
                {
                    if (i == 0)
                        j--;
                    else
                        i--;
                }

                warpPath[++warpPathIndex] = matrixPath[i, j];

            }
            while (i > 0 || j > 0);

            return warpPath;
        }
    }
}
