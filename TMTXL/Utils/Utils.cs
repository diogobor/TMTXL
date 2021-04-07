using System;
using System.Collections.Generic;
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

    }
}
