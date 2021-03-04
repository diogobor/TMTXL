using PatternTools;
using PatternTools.FastaParser;
using PatternTools.MSParserLight;
using PatternTools.PLP;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using TMTXL.Model;

namespace IsobaricAnalyzer
{
    public class IsobaricAnalyzerControl
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private List<(CSMSearchResult psm, MSUltraLight ms)> csmsToAnalyze { get; set; }
        private List<FastaItem> theFastaItems { get; set; }
        private PatternTools.CSML.Matrix purityCorrectionsMatrix { get; set; }

        /// <summary>
        /// Public variables
        /// </summary>
        public IsobaricParams myParams { get; set; }
        public Dictionary<string, double[]> signalAllNormalizationDictionary { get; set; }
        public Dictionary<string, double[]> signalIdentifiedNormalizationDictionary { get; set; }
        public List<CSMSearchResult> myCSMs { get; set; }
        public List<MSUltraLight> csmSpectra { get; set; }
        public List<string> rawfileIndex { get; set; }

        public bool stdOut_console { get; set; } = true;

        public void setCsmsToAnalyze(List<CSMSearchResult> psmList)
        {
            csmsToAnalyze = new List<(CSMSearchResult psm, MSUltraLight ms)>();


        }

        /// <summary>
        /// Method responsible for creating purity corrections matrix
        /// </summary>
        /// <param name="correctionData"></param>
        public void setPurityCorrectionsMatrix(List<List<double>> correctionData)
        {
            purityCorrectionsMatrix = new PatternTools.CSML.Matrix();

            if (correctionData.Count < 2)
            {
                throw new Exception("Please select the labeling kit in the purity correction tab.");
            }
            purityCorrectionsMatrix = IsobaricImpurityCorrection.GenerateInverseCorrectionMatrix(correctionData);
        }

        /// <summary>
        /// Method responsible for computing quantitation based on reporter ions (channels)
        /// </summary>
        public void computeQuantitation()
        {

            #region remove multiplex spectra

            this.removeMultiplexSpectra();

            #endregion

            #region initialize dictionary(ies) for the normalize reporter ions (channels) of all or identified spectra

            if (myParams.NormalizationAllSpectra)
            {
                this.setAllNormalizationDictionary();
            }

            if (myParams.NormalizationIdentifiedSpectra)
            {
                this.setIdentifiedNormalizationDictionary();
            }

            #endregion

            //compute the normalization for each spectrum
            this.computeQuantNormalization();
        }


        /// <summary>
        /// Method responsible for setting the normalization dictionary with the channels (reporter ions) of all spectra
        /// </summary>
        private void setAllNormalizationDictionary()
        {
            //Get signal from all spectra
            signalAllNormalizationDictionary = new Dictionary<string, double[]>();

            if ((myParams.InputFile == null || !File.Exists(myParams.InputFile.FullName)) && String.IsNullOrEmpty(myParams.RAWDirectory))
            {
                throw new Exception("Unable to find identification file");
            }

            List<FileInfo> rawFiles = null;

            if (myParams.InputFile != null && File.Exists(myParams.InputFile.FullName))
                rawFiles = myParams.InputFile.Directory.GetFiles("*.raw").ToList();
            else if (!String.IsNullOrEmpty(myParams.RAWDirectory))
            {
                DirectoryInfo folder = new DirectoryInfo(myParams.RAWDirectory);
                rawFiles = folder.GetFiles("*.raw", SearchOption.AllDirectories).ToList();
            }

            csmSpectra = new List<MSUltraLight>();
            foreach (FileInfo rawFile in rawFiles)
            {
                Console.WriteLine("Extracting data for " + rawFile.Name);

                List<MSUltraLight> spectraFromAThermoFile = ParserUltraLightRawFlash.Parse(rawFile.FullName, 2, -1, false, null, stdOut_console);
                spectraFromAThermoFile.RemoveAll(a => a.Ions == null);

                double[] totalSignal = new double[myParams.MarkerMZs.Count];

                Console.WriteLine("Computing quantitation...");

                object progress_lock = new object();
                int spectra_processed = 0;
                int old_progress = 0;
                double qtdSpectra = spectraFromAThermoFile.Count;

                //Get info for total signal normalization
                foreach (MSUltraLight ms in spectraFromAThermoFile)
                {
                    ms.Ions.RemoveAll(a => a.MZ > 400);

                    double[] thisQuantitation = GetIsobaricSignal(ms.Ions, myParams.MarkerMZs);
                    double maxSignal = thisQuantitation.Max();

                    // If a signal is less than the percentage specified in the ion threshold it should become 0.  
                    for (int i = 0; i < thisQuantitation.Length; i++)
                    {
                        if (thisQuantitation[i] < maxSignal * myParams.IonThreshold)
                        {
                            thisQuantitation[i] = 0;
                        }
                    }

                    //We can only correct for signal for those that have quantitation values in all places    
                    if (myParams.NormalizationPurityCorrection && (thisQuantitation.Count(a => a > 0) == myParams.MarkerMZs.Count))
                    {
                        thisQuantitation = IsobaricImpurityCorrection.CorrectForSignal(purityCorrectionsMatrix, thisQuantitation).ToArray();
                    }

                    for (int i = 0; i < thisQuantitation.Length; i++)
                    {
                        totalSignal[i] += thisQuantitation[i];
                    }

                    string current_fileNme = rawFile.Name.Substring(0, rawFile.Name.Length - rawFile.Extension.Length);
                    int rawFileIndex = rawfileIndex.IndexOf(current_fileNme);
                    if (rawFileIndex != -1)
                    {
                        CSMSearchResult cSMSearchResult = myCSMs.Where(a => a.scanNumber == ms.ScanNumber && a.fileIndex == rawFileIndex).FirstOrDefault();
                        if (cSMSearchResult != null)
                        {
                            cSMSearchResult.quantitation = thisQuantitation.ToList();
                            csmSpectra.Add(ms);
                        }
                    }

                    lock (progress_lock)
                    {
                        spectra_processed++;
                        int new_progress = (int)((double)spectra_processed / (qtdSpectra) * 100);
                        if (new_progress > old_progress)
                        {
                            old_progress = new_progress;

                            if (stdOut_console)
                            {
                                int currentLineCursor = Console.CursorTop;
                                Console.SetCursorPosition(0, Console.CursorTop);
                                Console.Write("Quantitation progress: " + old_progress + "%");
                                Console.SetCursorPosition(0, currentLineCursor);

                            }
                            else
                            {
                                Console.Write("Quantitation progress: " + old_progress + "%");
                            }
                        }
                    }
                }

                Console.Write("Done!");

                string theName = rawFile.Name.Substring(0, rawFile.Name.Length - 4);
                //theName += "ctxt";

                signalAllNormalizationDictionary.Add(theName, totalSignal);
            }
        }

        /// <summary>
        /// Method responsible for setting the normalization dictionary with the channels (reporter ions) of the identified spectra
        /// </summary>
        private void setIdentifiedNormalizationDictionary()
        {
            #region fill normalization dictionary

            #region prepare normalization Dictionary

            signalIdentifiedNormalizationDictionary = new Dictionary<string, double[]>();

            if (csmsToAnalyze == null)
                throw new Exception("There is no spectra to be normalized.");

            List<string> fileNames = csmsToAnalyze.Select(a => a.psm.fileName).Distinct().ToList();

            foreach (string fileName in fileNames)
            {
                signalIdentifiedNormalizationDictionary.Add(fileName, new double[myParams.MarkerMZs.Count]);
            }
            #endregion

            #endregion
        }

        /// <summary>
        /// Method responsible for removing multiplex spectra
        /// </summary>
        private void removeMultiplexSpectra()
        {
            if (myParams.YadaMultiplexCorrectionDir.Length > 0)
            {
                if (csmsToAnalyze == null)
                    throw new Exception("There is no spectra to be multiplexed.");

                YadaMultiplexFinder ymc = null;

                Console.WriteLine("Reading Yada results");
                ymc = new YadaMultiplexFinder(new DirectoryInfo(myParams.YadaMultiplexCorrectionDir));
                Console.WriteLine("Done loading Yada results");

                //Remove multiplexed spectra from sepro results
                int removedCounter = 0;

                foreach (KeyValuePair<string, List<int>> kvp in ymc.fileNameScanNumberMultiplexDictionary)
                {
                    Console.WriteLine("Removing multiplexed spectra for file :: " + kvp.Key);

                    string cleanName = kvp.Key.Substring(0, kvp.Key.Length - 4);
                    cleanName += ".sqt";
                    foreach (int scnNo in kvp.Value)
                    {
                        int index = csmsToAnalyze.FindIndex(a => a.psm.scanNumber == scnNo && a.psm.fileName.Equals(cleanName));

                        if (index >= 0)
                        {
                            Console.Write(csmsToAnalyze[index].psm.scanNumber + " ");

                            removedCounter++;
                            csmsToAnalyze.RemoveAt(index);
                        }
                    }

                    Console.WriteLine("\n");
                }

                Console.WriteLine("Done removing multiplexed spectra :: " + removedCounter);
            }
        }

        /// <summary>
        /// Method responsible for reading SEPro file and fill fastaItems and psmsToAnalyze objects
        /// </summary>
        private void readSeproFile()
        {
            Console.WriteLine("Loading SEPro2 file");

            //if (myParams.InputFile == null || !File.Exists(myParams.InputFile.FullName))
            //{
            //    throw new Exception("Unable to find SEPro file");
            //}

            //resultPackage = ResultPackage.Load(myParams.InputFile.FullName);
            //if (resultPackage.MySpectra == null || resultPackage.MySpectra.Count == 0)
            //    throw new Exception("Unable to find spectra in SEPro file.");

            //Console.WriteLine("Done reading SEPro result");
            //theFastaItems = resultPackage.MyProteins.MyProteinList.Select(a => new FastaItem(a.Locus, a.Sequence, a.Description)).ToList();
            //csmsToAnalyze = resultPackage.GetCombinedPSMMassSpectra();
        }

        /// <summary>
        /// Method responsible for applying the normalization (by taking into account all or only identified spectra) to the quantitation values
        /// </summary>
        private void computeQuantNormalization()
        {
            if (myParams.NormalizationIdentifiedSpectra || myParams.NormalizationAllSpectra)
            {
                if (myCSMs == null)
                    throw new Exception("There is no spectra to be quantified.");

                Console.WriteLine("Performing signal normalization taking into account " + myCSMs.Count + " scans.");

                foreach (CSMSearchResult csm in myCSMs)
                {
                    string fileName = rawfileIndex[csm.fileIndex];

                    for (int m = 0; m < myParams.MarkerMZs.Count; m++)
                    {
                        if (myParams.NormalizationIdentifiedSpectra)
                        {
                            csm.quantitation[m] /= signalIdentifiedNormalizationDictionary[fileName][m];
                        }
                        else
                        {
                            csm.quantitation[m] /= signalAllNormalizationDictionary[fileName][m];
                        }
                    }

                    if (csm.quantitation.Contains(double.NaN))
                    {
                        Console.WriteLine("Problems on signal of scan " + fileName + "\tScan No:" + csm.scanNumber);
                    }
                }

                Console.WriteLine("Done!");
            }
        }

        /// <summary>
        /// Method responsible for getting the isobaric signal
        /// </summary>
        /// <param name="theIons"></param>
        /// <param name="isoMasses"></param>
        /// <returns></returns>
        private double[] GetIsobaricSignal(List<(double, double)> theIons, List<double> isoMasses)
        {
            double[] sig = new double[isoMasses.Count];

            for (int i = 0; i < sig.Length; i++)
            {
                List<(double, double)> acceptableIons = theIons.FindAll(a => Math.Abs(PatternTools.pTools.PPM(a.Item1, isoMasses[i])) < myParams.MarkerPPMTolerance);

                double sum = 0;
                if (acceptableIons.Count > 0)
                {
                    sum = acceptableIons.Sum(a => a.Item2);
                }
                sig[i] = sum;
            }

            return sig;
        }

        /// <summary>
        /// Method responsible for generating quantitation report (peptide quant report or plp file)
        /// </summary>
        /// <param name="fileName"></param>
        public void generateQuantitationReport(string fileName)
        {
            if (myParams.AnalysisType)
            {
                //Generate peptide quantitation report
                this.generatePeptideQuantReport(fileName);
            }
        }

        /// <summary>
        /// Method responsible for generating peptide quantitation report file
        /// </summary>
        /// <param name="fileName"></param>
        private void generatePeptideQuantReport(string fileName)
        {
            //Peptide Analysis

            //Write Peptide Analysis
            StreamWriter sw = new StreamWriter(fileName);

            //Eliminate problematic quants
            int removed = csmsToAnalyze.RemoveAll(a => a.psm.quantitation is null);
            Console.WriteLine("Problematic scans removed: " + removed);

            //var pepDic = from scn in csmsToAnalyze
            //             group scn by scn.psm.PeptideSequence


            //             into groupedSequences
            //             select new { PeptideSequence = groupedSequences.Key, TheScans = groupedSequences.ToList() };

            //foreach (var pep in pepDic)
            //{
            //    sw.WriteLine("Peptide:" + pep.PeptideSequence + "\tSpecCounts:" + pep.TheScans.Count);

            //    foreach (var sqt in pep.TheScans)
            //    {
            //        sw.WriteLine(sqt.psm.FileName + "." + sqt.psm.ScanNumber + "." + sqt.psm.ScanNumber + "." + sqt.psm.ChargeState + "\t" + string.Join("\t", sqt.psm.Quantitation[0]));
            //    }
            //}


            //And now write the Fasta
            sw.WriteLine("#Fasta Items");
            foreach (FastaItem fastaItem in theFastaItems)
            {
                sw.WriteLine(">" + fastaItem.SequenceIdentifier + " " + fastaItem.Description);
                sw.WriteLine(fastaItem.Sequence);
            }

            sw.Close();
        }
    }
}
