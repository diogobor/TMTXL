using PatternTools;
using PatternTools.FastaParser;
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
using TMTXL.Control;
using TMTXL.Model;

namespace IsobaricAnalyzer
{
    public class IsobaricAnalyzerControl
    {


        /// <summary>
        /// Private variables
        /// </summary>
        private List<(CSMSearchResult psm, PatternTools.MSParserLight.MSUltraLight ms)> csmsToAnalyze { get; set; }
        private List<FastaItem> theFastaItems { get; set; }
        private PatternTools.CSML.Matrix purityCorrectionsMatrix { get; set; }

        /// <summary>
        /// Public variables
        /// </summary>
        public IsobaricParams myParams { get; set; }
        public Dictionary<string, double[]> signalAllNormalizationDictionary { get; set; }
        public Dictionary<string, double[]> signalIdentifiedNormalizationDictionary { get; set; }
        public ResultsPackage resultsPackage { get; set; }

        public bool stdOut_console { get; set; } = true;

        public void setCsmsToAnalyze(List<CSMSearchResult> psmList)
        {
            csmsToAnalyze = new List<(CSMSearchResult psm, PatternTools.MSParserLight.MSUltraLight ms)>();


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

            //if (myParams.NormalizationAllSpectra)
            //{
            this.setAllNormalizationDictionary();
            //}

            if (myParams.NormalizationIdentifiedSpectra)
            {
                this.setIdentifiedNormalizationDictionary();
            }

            #endregion

            //compute the quantitation for each spectrum
            this.computeCSMQuant();

            //compute the quantitation of all xls
            this.computeXLQuantitation();

            //compute the quantitation of all xls with the same reaction site position
            this.computeResidueQuantitation();

            //compute the quantitation of all genes
            this.computePPIQuantitation();
        }

        private void computePPIQuantitation()
        {
            if (resultsPackage == null || resultsPackage.XLSearchResults == null || resultsPackage.XLSearchResults.Count == 0) return;

            Console.WriteLine("Computing PPI quantitation...");

            object progress_lock = new object();
            int ppi_processed = 0;
            int old_progress = 0;
            double qtdPpi = resultsPackage.PPIResults.Count();

            foreach (ProteinProteinInteraction ppi in resultsPackage.PPIResults)
            {
                //double[] thisQuantitation = new double[myParams.MarkerMZs.Count];

                var xlDic = from csm in resultsPackage.CSMSearchResults.Where(a => a.genes_alpha.Contains(ppi.gene_a) && a.genes_beta.Contains(ppi.gene_b))
                            group csm by new
                            {
                                csm.alpha_peptide,
                                csm.beta_peptide,
                                csm.alpha_pept_xl_pos,
                                csm.beta_pept_xl_pos
                            }
                         into groupedSeq
                            select new { xl = groupedSeq.Key, csms = groupedSeq.ToList() };

                foreach (var xl in xlDic)
                {
                    ppi.specCount = xl.csms.Count;
                    ppi.log2FoldChange = xl.csms[0].log2FoldChange;
                    ppi.pValue = xl.csms[0].pValue;

                    if (xl.csms.Count > 1)
                    {
                        var folds = xl.csms.Select(a => a.log2FoldChange).ToList();
                        ppi.log2FoldChange = folds.Average();
                        ppi.pValue = folds.Count > 1 ? IsobaricUtils.computeOneSampleTtest(folds) : xl.csms[0].pValue;
                    }
                }


                //List<CSMSearchResult> csm_results = resultsPackage.CSMSearchResults.Where(a => a.genes_alpha.Contains(ppi.gene_a) && a.genes_beta.Contains(ppi.gene_b)).ToList();

                //if (csm_results.Count > 0)
                //{
                //    if (csm_results.Count > 1)
                //    {
                //        for (int i = 0; i < myParams.MarkerMZs.Count; i++)
                //        {
                //            var orderedQuant = csm_results.Select(a => a.quantitation[i]).OrderBy(p => p);
                //            int count = orderedQuant.Count();
                //            double median = orderedQuant.ElementAt(count / 2) + orderedQuant.ElementAt((count - 1) / 2);
                //            median /= 2;
                //            thisQuantitation[i] = median;
                //        }
                //    }
                //    else
                //        ppi.quantitation = csm_results[0].quantitation;
                //}
                //ppi.quantitation = thisQuantitation.ToList();

                lock (progress_lock)
                {
                    ppi_processed++;
                    int new_progress = (int)((double)ppi_processed / (qtdPpi) * 100);
                    if (new_progress > old_progress)
                    {
                        old_progress = new_progress;

                        if (stdOut_console)
                        {
                            int currentLineCursor = Console.CursorTop;
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write("PPI Quantitation progress: " + old_progress + "%");
                            Console.SetCursorPosition(0, currentLineCursor);

                        }
                        else
                        {
                            Console.Write("PPI Quantitation progress: " + old_progress + "%");
                        }
                    }
                }
            }
        }

        private void computeResidueQuantitation()
        {
            if (resultsPackage == null || resultsPackage.CSMSearchResults == null || resultsPackage.CSMSearchResults.Count == 0) return;

            Console.WriteLine("Computing Residues quantitation...");

            var residueDic = from csm in resultsPackage.CSMSearchResults
                             group csm by new
                             {
                                 ptn_a = csm.proteins_alpha.ToList<string>()[0],
                                 ptn_b = csm.proteins_beta.ToList<string>()[0],
                                 csm.alpha_pept_xl_pos,
                                 csm.beta_pept_xl_pos
                             }
                         into groupedSeq
                             select new { xl = groupedSeq.Key, csms = groupedSeq.ToList() };


            object progress_lock = new object();
            int xl_processed = 0;
            int old_progress = 0;
            double qtdXL = residueDic.Count();

            foreach (var xl in residueDic)
            {
                List<string> alpha_ptns = new List<string>();
                List<string> beta_ptns = new List<string>();
                List<string> alpha_genes = new List<string>();
                List<string> beta_genes = new List<string>();
                xl.csms.ForEach(a =>
                {
                    alpha_ptns.AddRange(a.proteins_alpha);
                    beta_ptns.AddRange(a.proteins_beta);
                    alpha_genes.AddRange(a.genes_alpha);
                    beta_genes.AddRange(a.genes_beta);
                }
                );

                CSMSearchResult residueSr = new CSMSearchResult(xl.csms[0]._index, xl.csms[0].fileIndex, xl.csms[0].scanNumber, xl.csms[0].charge, xl.csms[0].precursor_mass, xl.csms[0].alpha_peptide, xl.csms[0].beta_peptide, xl.csms[0].alpha_pos_xl, xl.csms[0].beta_pos_xl, xl.csms[0].alpha_pept_xl_pos, xl.csms[0].beta_pept_xl_pos, alpha_ptns.Distinct().ToList(), beta_ptns.Distinct().ToList(), xl.csms[0].peptide_alpha_mass, xl.csms[0].peptide_beta_mass, xl.csms[0].peptide_alpha_score, xl.csms[0].peptide_beta_score, alpha_genes.Distinct().ToList(), beta_genes.Distinct().ToList());
                residueSr.quantitation = xl.csms[0].quantitation;
                residueSr.specCount = xl.csms.Count;
                residueSr.log2FoldChange = xl.csms[0].log2FoldChange;
                residueSr.pValue = xl.csms[0].pValue;

                if (xl.csms.Count > 1)
                {
                    var folds = xl.csms.Select(a => a.log2FoldChange).ToList();
                    residueSr.log2FoldChange = folds.Average();
                    residueSr.pValue = folds.Count > 1 ? IsobaricUtils.computeOneSampleTtest(folds) : xl.csms[0].pValue;
                }

                resultsPackage.ResidueSearchResults.Add(residueSr);

                lock (progress_lock)
                {
                    xl_processed++;
                    int new_progress = (int)((double)xl_processed / (qtdXL) * 100);
                    if (new_progress > old_progress)
                    {
                        old_progress = new_progress;

                        if (stdOut_console)
                        {
                            int currentLineCursor = Console.CursorTop;
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write("Residue Quantitation progress: " + old_progress + "%");
                            Console.SetCursorPosition(0, currentLineCursor);

                        }
                        else
                        {
                            Console.Write("Residue Quantitation progress: " + old_progress + "%");
                        }
                    }
                }
            }
        }

        private void computeXLQuantitation()
        {
            if (resultsPackage == null || resultsPackage.CSMSearchResults == null || resultsPackage.CSMSearchResults.Count == 0) return;

            Console.WriteLine("Computing XL quantitation...");

            var xlDic = from csm in resultsPackage.CSMSearchResults
                        group csm by new
                        {
                            csm.alpha_peptide,
                            csm.beta_peptide,
                            csm.alpha_pept_xl_pos,
                            csm.beta_pept_xl_pos
                        }
                         into groupedSeq
                        select new { xl = groupedSeq.Key, csms = groupedSeq.ToList() };

            resultsPackage.XLSearchResults = new List<CSMSearchResult>();

            object progress_lock = new object();
            int xl_processed = 0;
            int old_progress = 0;
            double qtdXL = xlDic.Count();

            foreach (var xl in xlDic)
            {
                List<string> alpha_ptns = new List<string>();
                List<string> beta_ptns = new List<string>();
                List<string> alpha_genes = new List<string>();
                List<string> beta_genes = new List<string>();
                xl.csms.ForEach(a =>
                {
                    alpha_ptns.AddRange(a.proteins_alpha);
                    beta_ptns.AddRange(a.proteins_beta);
                    alpha_genes.AddRange(a.genes_alpha);
                    beta_genes.AddRange(a.genes_beta);
                }
                );

                CSMSearchResult xlSr = new CSMSearchResult(xl.csms[0]._index, xl.csms[0].fileIndex, xl.csms[0].scanNumber, xl.csms[0].charge, xl.csms[0].precursor_mass, xl.csms[0].alpha_peptide, xl.csms[0].beta_peptide, xl.csms[0].alpha_pos_xl, xl.csms[0].beta_pos_xl, xl.csms[0].alpha_pept_xl_pos, xl.csms[0].beta_pept_xl_pos, alpha_ptns.Distinct().ToList(), beta_ptns.Distinct().ToList(), xl.csms[0].peptide_alpha_mass, xl.csms[0].peptide_beta_mass, xl.csms[0].peptide_alpha_score, xl.csms[0].peptide_beta_score, alpha_genes.Distinct().ToList(), beta_genes.Distinct().ToList());
                xlSr.quantitation = xl.csms[0].quantitation;
                xlSr.specCount = xl.csms.Count;
                xlSr.log2FoldChange = xl.csms[0].log2FoldChange;
                xlSr.pValue = xl.csms[0].pValue;

                if (xl.csms.Count > 1)
                {
                    //double[] thisQuantitation = new double[myParams.MarkerMZs.Count];

                    //for (int i = 0; i < myParams.MarkerMZs.Count; i++)
                    //{
                    //    var orderedQuant = xl.csms.Select(a => a.quantitation[i]).OrderBy(p => p);
                    //    int count = orderedQuant.Count();
                    //    double median = orderedQuant.ElementAt(count / 2) + orderedQuant.ElementAt((count - 1) / 2);
                    //    median /= 2;
                    //    thisQuantitation[i] = median;
                    //}
                    //xlSr.quantitation = thisQuantitation.ToList();

                    var folds = xl.csms.Select(a => a.log2FoldChange).ToList();
                    xlSr.log2FoldChange = folds.Average();
                    xlSr.pValue = folds.Count > 1 ? IsobaricUtils.computeOneSampleTtest(folds) : xl.csms[0].pValue;
                }

                resultsPackage.XLSearchResults.Add(xlSr);

                lock (progress_lock)
                {
                    xl_processed++;
                    int new_progress = (int)((double)xl_processed / (qtdXL) * 100);
                    if (new_progress > old_progress)
                    {
                        old_progress = new_progress;

                        if (stdOut_console)
                        {
                            int currentLineCursor = Console.CursorTop;
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write("XL Quantitation progress: " + old_progress + "%");
                            Console.SetCursorPosition(0, currentLineCursor);

                        }
                        else
                        {
                            Console.Write("XL Quantitation progress: " + old_progress + "%");
                        }
                    }
                }
            }
        }
        private void computeCSMQuant()
        {
            //input - class labels
            List<int> classLabelList = new() { 1, 1, 1, 1, 1, 2, 2, 2, 2, 2 };//make this global

            if (resultsPackage == null || resultsPackage.CSMSearchResults == null)
                throw new Exception("There is no spectra to be quantified.");

            Console.WriteLine("Performing quantitation taking into account " + resultsPackage.CSMSearchResults.Count + " scans.");

            foreach (CSMSearchResult csm in resultsPackage.CSMSearchResults)
            {
                string fileName = resultsPackage.FileNameIndex[csm.fileIndex];

                for (int m = 0; m < myParams.MarkerMZs.Count; m++)
                {
                    if (myParams.NormalizationIdentifiedSpectra)
                    {
                        csm.quantitation[m] /= signalIdentifiedNormalizationDictionary[fileName][m];
                    }
                    else if (myParams.NormalizationAllSpectra)
                    {
                        csm.quantitation[m] /= signalAllNormalizationDictionary[fileName][m];
                    }
                }

                if (csm.quantitation.Contains(double.NaN))
                {
                    Console.WriteLine("ERROR: Problems on signal of scan " + fileName + "\tScan No:" + csm.scanNumber);
                }
                else
                {
                    csm.avg_notNull_1 = IsobaricUtils.computeAVG(csm.quantitation, 1, classLabelList);
                    csm.avg_notNull_2 = IsobaricUtils.computeAVG(csm.quantitation, 2, classLabelList);
                    csm.log2FoldChange = Math.Log2(csm.avg_notNull_1 / csm.avg_notNull_2);
                    csm.pValue = IsobaricUtils.computeTtest(csm.quantitation);
                }
            }

            Console.WriteLine("Done!");
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

            resultsPackage.Spectra = new List<MSUltraLight>();
            foreach (FileInfo rawFile in rawFiles)
            {
                Console.WriteLine("Extracting data for " + rawFile.Name);

                string current_fileNme = rawFile.Name.Substring(0, rawFile.Name.Length - rawFile.Extension.Length);
                int rawFileIndex = resultsPackage.FileNameIndex.IndexOf(current_fileNme);

                List<MSUltraLight> spectraFromAThermoFile = (from ms in PatternTools.MSParserLight.ParserUltraLightRawFlash.Parse(rawFile.FullName, 2, (short)rawFileIndex, false, null, stdOut_console).AsParallel()
                                                             select new MSUltraLight()
                                                             {
                                                                 ActivationType = ms.ActivationType,
                                                                 CromatographyRetentionTime = ms.CromatographyRetentionTime,
                                                                 FileNameIndex = ms.FileNameIndex,
                                                                 InstrumentType = ms.InstrumentType,
                                                                 Ions = (from ion in ms.Ions.AsParallel()
                                                                         select Tuple.Create(ion.Item1, ion.Item2)).ToList(),
                                                                 MSLevel = ms.MSLevel,
                                                                 Precursors = (from ion in ms.Precursors.AsParallel()
                                                                               select Tuple.Create(ion.Item1, ion.Item2)).ToList(),

                                                                 ScanNumber = ms.ScanNumber,
                                                             }).ToList();
                spectraFromAThermoFile.RemoveAll(a => a.Ions == null);

                double[] totalSignal = new double[myParams.MarkerMZs.Count];

                Console.WriteLine("Computing CSM quantitation...");

                object progress_lock = new object();
                int spectra_processed = 0;
                int old_progress = 0;
                double qtdSpectra = spectraFromAThermoFile.Count;

                //Get info for total signal normalization
                foreach (MSUltraLight ms in spectraFromAThermoFile)
                {
                    double[] thisQuantitation = GetIsobaricSignal(ms.Ions.Where(a => a.Item1 < 200).ToList(), myParams.MarkerMZs);
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

                    if (rawFileIndex != -1)
                    {
                        CSMSearchResult cSMSearchResult = resultsPackage.CSMSearchResults.Where(a => a.scanNumber == ms.ScanNumber && a.fileIndex == rawFileIndex).FirstOrDefault();
                        if (cSMSearchResult != null)
                        {
                            cSMSearchResult.quantitation = thisQuantitation.ToList();
                            resultsPackage.Spectra.Add(ms);
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
                                Console.Write("CSM Quantitation progress: " + old_progress + "%");
                                Console.SetCursorPosition(0, currentLineCursor);

                            }
                            else
                            {
                                Console.Write("CSM Quantitation progress: " + old_progress + "%");
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

        /// <summary>
        /// Method responsible for getting the isobaric signal
        /// </summary>
        /// <param name="theIons"></param>
        /// <param name="isoMasses"></param>
        /// <returns></returns>
        private double[] GetIsobaricSignal(List<Tuple<double, double>> theIons, List<double> isoMasses)
        {
            double[] sig = new double[isoMasses.Count];

            for (int i = 0; i < sig.Length; i++)
            {
                List<Tuple<double, double>> acceptableIons = theIons.FindAll(a => Math.Abs(PatternTools.pTools.PPM(a.Item1, isoMasses[i])) < myParams.MarkerPPMTolerance);

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
