using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsobaricAnalyzer
{
    public static class IsobaricUtils
    {
        /// <summary>
        /// Method responsible for computing pValue of a distribution
        /// </summary>
        /// <param name="quantitation"></param>
        /// <returns>pValue</returns>
        public static double computeTtest(List<double> quantitation)
        {
            List<double> thePosNormalized = quantitation.Take(quantitation.Count / 2).ToList();
            List<double> theNegNormalized = quantitation.Skip(quantitation.Count / 2).Take(quantitation.Count / 2).ToList();
            double bothTails;
            double leftTail;
            double rightTail;

            alglib.studentttest2(thePosNormalized.ToArray(), thePosNormalized.Count, theNegNormalized.ToArray(), theNegNormalized.Count, out bothTails, out leftTail, out rightTail);

            double pvalue = bothTails / 2;

            return pvalue;
        }

        /// <summary>
        /// Method responsible for computing average of population not considering null values
        /// </summary>
        /// <param name="quantitation"></param>
        /// <param name="classLabel"></param>
        /// <param name="classLabelList"></param>
        /// <returns>average</returns>
        public static double computeAVG(List<double> quantitation, int classLabel, List<int> classLabelList)
        {
            List<double> thisQuant = new();

            for (int i = 0; i < quantitation.Count; i++)
            {
                if (classLabelList[i] == classLabel && quantitation[i] > 0)
                {
                    thisQuant.Add(quantitation[i]);
                }
            }

            return thisQuant.Count > 0 ? thisQuant.Average() : 0;
        }

        /// <summary>
        /// Method responsible for computing the t test based on a priori probability
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="prioriProbability"></param>
        /// <returns>p-value</returns>
        public static double computeOneSampleTtest(List<double> dataset, double prioriProbability = 0)
        {
            double bothTails;
            double leftTail;
            double rightTail;

            alglib.studentttest1(dataset.ToArray(), dataset.Count, prioriProbability, out bothTails, out leftTail, out rightTail);

            if (bothTails < 0.001)
            {
                bothTails = 0.001;
            }

            return bothTails;
        }
    }
}
