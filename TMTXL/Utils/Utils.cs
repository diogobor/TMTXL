using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTXL.Utils
{
    public static class Utils
    {

        public static int SPEC_COUNT { get; set; } = 2;
        public static int MIN_CROSSLINKEDPEPTIDES { get; set; } = 2;
        public static double FOLD_CHANGE_CUTOFF { get; set; } = 1;
        public static double PVALUE_CUTOFF { get; set; } = 0.05;

        /// <summary>
        /// Method responsible for truncating large numbers
        /// </summary>
        /// <param name="input"></param>
        /// <param name="places"></param>
        /// <returns>truncate number</returns>
        public static double RoundUp(double input, int places = 10)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(places));
            return Math.Ceiling(input * multiplier) / multiplier;
        }

        /// <summary>
        /// Method responsible for computing the median
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Median(List<double> values)
        {
            double median = 0;
            var orderedQuant = values.OrderBy(p => p);
            int count = orderedQuant.Count();
            median = orderedQuant.ElementAt(count / 2) + orderedQuant.ElementAt((count - 1) / 2);
            median /= 2;
            
            return median;
        }

        /// <summary>
        /// Method responsible for figuring out the lowest value of a list
        /// </summary>
        /// <param name="self"></param>
        /// <returns>lowest value of the list</returns>
        public static int IndexOfMin(List<double> self)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            if (self.Count == 0)
            {
                throw new ArgumentException("List is empty.", "self");
            }

            double min = self[0];
            int minIndex = 0;

            for (int i = 1; i < self.Count; ++i)
            {
                if (self[i] < min)
                {
                    min = self[i];
                    minIndex = i;
                }
            }

            return minIndex;
        }

        public static string RemoveExtension(string fileName)
        {
            FileInfo rawFile = new FileInfo(fileName);
            string current_fileName = rawFile.Name.Substring(0, rawFile.Name.Length - rawFile.Extension.Length);
            return current_fileName;
        }

    }
}
