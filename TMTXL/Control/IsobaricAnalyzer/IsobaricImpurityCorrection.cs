using PatternTools.CSML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsobaricAnalyzer
{
    public static class IsobaricImpurityCorrection
    {
        public static void ITRAQExample()
        {
            //We should enter the monoisotopic correction factor as -1
            double[] signal = new double[4] { 1000, 1000, 1000, 1000 };

            List<List<double>> correctionFactors = new List<List<double>>()
            {   new List<double>() { 0, 1, -1, 5.9, 0.2 },  //114
                new List<double>() { 0, 2, -1, 5.6, 0.1 },  //115
                new List<double>() { 0, 3, -1, 4.5, 0.1 },  //116
                new List<double>() { 0, 4, -1, 3.5, 0.1 }   //117
            };


            Matrix cm = GenerateInverseCorrectionMatrix(correctionFactors);
            List<double> correctedSignal = CorrectForSignal(cm, new double[] { 1000, 1000, 1000, 1000 });


            Console.WriteLine("Corrected signal: " + string.Join(", ", correctedSignal));

        }

        public static List<double> CorrectForSignal(PatternTools.CSML.Matrix cInverse, double[] signal)
        {
            //Mcorrected = c^-1 * signal
            PatternTools.CSML.Matrix correctedS = cInverse * new PatternTools.CSML.Matrix(signal);
            return correctedS.GetRow(0);

        }


        /// <summary>
        /// Each row should contain the correction factors in numbers that range from 1 to 100
        /// The value for the monoisotopic should be entered as -1
        /// </summary>
        /// <param name="correctionTable"></param>
        /// <returns></returns>
        public static PatternTools.CSML.Matrix GenerateInverseCorrectionMatrix(List<List<double>> correctionTable)
        {
            //We now need to normalize the -1 so the sum adds to 100
            foreach (List<double> l in correctionTable)
            {
                double sum = l.Sum() + 1;
                int index = l.FindIndex(a => a == -1);
                l[index] = 100 - sum;
            }


            //Generate Correction Matrix
            //Prepare the coeficient matrix
            double[,] coeficientMatrix = new double[correctionTable.Count, correctionTable.Count];
            int counter = -1;
            for (int i = 0; i < correctionTable.Count; i++)
            {
                counter++;
                for (int j = 0; j < correctionTable[0].Count; j++)
                {
                    try
                    {
                        coeficientMatrix[j - 2 + counter, i] = correctionTable[i][j];
                    }
                    catch (Exception e)
                    {

                        //Console.WriteLine(e.Message + "\n i = {0}, j = {1}", i, j);
                    }
                }
            }




            //Mcorrected = c^-1 * signal
            PatternTools.CSML.Matrix c = new PatternTools.CSML.Matrix(coeficientMatrix);
            PatternTools.CSML.Matrix cInverse = c.Inverse();

            cInverse *= 100;

            return cInverse;
        }
    }
}
