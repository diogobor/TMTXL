using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TMTXL.Model
{
    [Serializable]
    [ProtoContract]
    public class MSUltraLight
    {
        [ProtoMember(1)]
        public double CromatographyRetentionTime { get; set; }
        [ProtoMember(2)]
        public int ScanNumber { get; set; }
        [ProtoMember(3)]
        public string ScanHeader { get; set; }

        /// <summary>
        /// -1 for NA, 1 for FTMS, 2 for ITMS, 3 for TOF, 4 for quadrupole
        /// </summary>
        [ProtoMember(4)]
        public short InstrumentType { get; set; }

        /// <summary>
        /// 1 for CID, 2 for HCD, 3 for ETD, 4 for ECD, 5 for MPD, 6 for Not Found, 7 for PQD
        /// </summary>
        [ProtoMember(5)]
        public short ActivationType { get; set; }
        [ProtoMember(6)]
        public short MSLevel { get; set; }
        //<MZ, Intensity>
        [ProtoMember(7)]
        public List<Tuple<double, double>> Ions { get; set; }

        public static string GetInstrumentType(short theType)
        {
            switch (theType)
            {
                case 1:
                    return ("FTMS");
                case 2:
                    return ("ITMS");
                case 3:
                    return ("TOF");
                case 4:
                    return ("Quadrupole");
                default:
                    throw new Exception("Unknown instrument type.");
            }
        }

        public static string GetActivationType(short theType)
        {
            switch (theType)
            {
                case 1:
                    return ("CID");
                case 2:
                    return ("HCD");
                case 3:
                    return ("ETD");
                case 4:
                    return ("ECD");
                case 5:
                    return ("MPD");
                case 6:
                    return ("Not found");
                case 7:
                    return ("PQD");
                default:
                    throw new Exception("Unknown activation type.");
            }
        }

        /// <summary>
        /// An index to represent the file where this spectrum was extracted from.
        /// </summary>
        [ProtoMember(8)]
        public short FileNameIndex { get; set; }


        /// <summary>
        /// MZ, Z; a Z of 0 means it is unknown
        /// </summary>
        [ProtoMember(9)]
        public List<Tuple<double, short>> Precursors { get; set; }
        [ProtoMember(10)]
        public int PrecursorScanNumber { get; set; }


        public MSUltraLight(double chromatograpgyRetentionTime,
                            int scanNumber,
                            List<Tuple<double, double>> ions,
                            List<Tuple<double, short>> precursors,
                            double precursorIntensity,
                            short instrumentTye,
                            short mslevel,
                            short fileNameIndex = -1)
        {
            this.CromatographyRetentionTime = chromatograpgyRetentionTime;
            this.ScanNumber = scanNumber;
            this.Ions = ions;
            this.FileNameIndex = fileNameIndex;
            Precursors = precursors;
            MSLevel = mslevel;
            InstrumentType = instrumentTye;
            ActivationType = -1;

        }

        /// <summary>
        /// Normalize intensities to unit vector so that square norm equals one.
        /// </summary>
        public void NormalizeIntensities()
        {
            double denominator = Math.Sqrt(Ions.Sum(a => Math.Pow(a.Item2, 2)));
            Ions = Ions.Select(a => Tuple.Create(a.Item1, a.Item2 / denominator)).ToList();
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public MSUltraLight()
        {

        }

        public List<string> GetZLines()
        {
            if (MSLevel > 1)
            {
                StringBuilder zLinesSb = new StringBuilder();

                foreach (var p in Precursors)
                {
                    zLinesSb.Append("Z\t" + p.Item2 + "\t" + PatternTools.pTools.DechargeMSPeakToPlus1(p.Item1, p.Item2).ToString());
                }

                List<string> zLines = new List<string>();
                zLines = Regex.Split(zLinesSb.ToString(), "\r\n").ToList();
                zLines.RemoveAll(a => String.IsNullOrEmpty(a));
                return zLines;

            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// This method will analyze the spectrum in bins of windowSize and for each window will keep the ionsInWindow most intense ions
        /// </summary>
        /// <param name="windowSize"></param>
        /// <param name="ionsInWindow"></param>
        /// <returns></returns>
        public List<Tuple<double, double>> GetIonsCleaned(float windowSize, int ionsInWindow)
        {

            List<Tuple<double, double>> cleanedMS = new List<Tuple<double, double>>();

            for (float window = 0; window <= Ions.Max(a => a.Item1); window += windowSize)
            {
                List<Tuple<double, double>> ions = Ions.FindAll(a => a.Item1 > window && a.Item1 <= window + windowSize).ToList();
                ions.Sort((a, b) => b.Item2.CompareTo(a.Item2));

                cleanedMS.AddRange(ions.Take(ionsInWindow).ToList());

            }

            cleanedMS.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            return cleanedMS;
        }
    }
}
