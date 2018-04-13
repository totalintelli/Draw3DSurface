using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Accord.Statistics.Distributions.Multivariate;

namespace Draw3DSurface
{
    public class Calculation
    {
        /// <summary>
        /// Probability density를 구한다. (Accord.NET Framework 라이브러리 사용)
        /// </summary>
        /// <param name="observations"></param>
        public static double[,] GetProbabilityDensity(double[][] observations, MultivariateNormalDistribution dist)
        {
            double[,] probabilityDensity = new double[observations.Length, observations.Length]; // Probability Density Function
            for (int i = 0; i < observations.Length; i++)
            {
                for (int j = 0; j < observations.Length; j++)
                {
                    probabilityDensity[i,j] = dist.ProbabilityDensityFunction(observations[i]);
                }
            }
            return probabilityDensity;
        }
    }
}
