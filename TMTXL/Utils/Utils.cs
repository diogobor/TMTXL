using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTXL.Utils
{
    public static class Utils
    {
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
