using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IsobaricAnalyzer
{
    public class IsobaricParams
    {
        public FileInfo InputFile { get; set; }
        public string RAWDirectory { get; set; }
        public string YadaMultiplexCorrectionDir { get; set; }
        public int MarkerPPMTolerance { get; set; }
        public double IonThreshold { get; set; }
        public List<double> MarkerMZs { get; set; }
        public List<int> ClassLabels { get; set; }
        public bool NormalizationIdentifiedSpectra { get; set; }
        public bool NormalizationAllSpectra { get; set; }
        public bool NormalizationPurityCorrection { get; set; }
        /// <summary>
        /// true = Peptide Quantitation Report
        /// false = PatternLabProject
        /// </summary>
        public bool AnalysisType { get; set; }
        public bool PatternLabProjectOnlyUniquePeptides { get; set; }
        public bool SPSMS3 { get; set; }
    }
}
