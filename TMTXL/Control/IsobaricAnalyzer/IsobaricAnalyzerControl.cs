using PatternTools;
using PatternTools.FastaParser;
using PatternTools.PLP;
using PatternTools.YADA;
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

            this.processRawFiles();

            #region initialize dictionary(ies) for the normalize reporter ions (channels) of all or identified spectra


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

        /// <summary>
        /// Method responsible for quantifying PPIs
        /// </summary>
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
                List<CSMSearchResult> xlDic = resultsPackage.CSMSearchResults.Where(a => a.genes_alpha.Contains(ppi.gene_a) && a.genes_beta.Contains(ppi.gene_b)).ToList();
                xlDic.RemoveAll(a => a.log2FoldChange.Any(b => double.IsNaN(b)));

                if (xlDic.Count > 0)
                {
                    ppi.cSMs = xlDic;
                    ppi.specCount = xlDic.Count;
                    ppi.log2FoldChange = xlDic[0].log2FoldChange;
                    ppi.pValue = xlDic[0].pValue;

                    if (xlDic.Count > 1)
                    {
                        int qtdFoldChange = xlDic[0].log2FoldChange.Count;
                        ppi.log2FoldChange = new();
                        ppi.pValue = new();
                        for (int i = 0; i < qtdFoldChange; i++)
                        {
                            var folds = xlDic.Select(a => a.log2FoldChange[i]).ToList();

                            double median = 0;
                            if (folds.Count > 0)
                            {
                                if (folds.Count == 1)
                                    median = folds[0];
                                else
                                {
                                    var orderedQuant = folds.OrderBy(p => p);
                                    int count = orderedQuant.Count();
                                    median = orderedQuant.ElementAt(count / 2) + orderedQuant.ElementAt((count - 1) / 2);
                                    median /= 2;
                                }
                            }

                            ppi.log2FoldChange.Add(median);
                            ppi.pValue.Add(folds.Count > 1 ? IsobaricUtils.computeOneSampleTtest(folds) : xlDic[0].pValue[i]);
                        }
                    }
                }

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

        /// <summary>
        /// Method responsible for quantifying Residues
        /// </summary>
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
                xl.csms.RemoveAll(a => a.log2FoldChange.Any(b => double.IsNaN(b)));
                if (xl.csms.Count == 0) continue;

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

                XLSearchResult residueSr = new XLSearchResult(xl.csms);
                residueSr.quantitation = xl.csms[0].quantitation;
                residueSr.log2FoldChange = xl.csms[0].log2FoldChange;
                residueSr.pValue = xl.csms[0].pValue;

                if (xl.csms.Count > 1)
                {
                    int qtdFoldChange = xl.csms[0].log2FoldChange.Count;
                    residueSr.log2FoldChange = new();
                    residueSr.pValue = new();
                    for (int i = 0; i < qtdFoldChange; i++)
                    {
                        var folds = xl.csms.Select(a => a.log2FoldChange[i]).ToList();

                        double median = 0;
                        if (folds.Count > 0)
                        {
                            if (folds.Count == 1)
                                median = folds[0];
                            else
                            {
                                var orderedQuant = folds.OrderBy(p => p);
                                int count = orderedQuant.Count();
                                median = orderedQuant.ElementAt(count / 2) + orderedQuant.ElementAt((count - 1) / 2);
                                median /= 2;
                            }
                        }

                        residueSr.log2FoldChange.Add(median);
                        residueSr.pValue.Add(folds.Count > 1 ? IsobaricUtils.computeOneSampleTtest(folds) : xl.csms[0].pValue[i]);
                    }
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

        /// <summary>
        /// Method responsible for quantifying XLs
        /// </summary>
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

            resultsPackage.XLSearchResults = new List<XLSearchResult>();

            object progress_lock = new object();
            int xl_processed = 0;
            int old_progress = 0;
            double qtdXL = xlDic.Count();

            foreach (var xl in xlDic)
            {
                xl.csms.RemoveAll(a => a.log2FoldChange.Any(b => double.IsNaN(b)));
                if (xl.csms.Count == 0) continue;

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

                XLSearchResult xlSr = new XLSearchResult(xl.csms);
                xlSr.quantitation = xl.csms[0].quantitation;
                xlSr.log2FoldChange = xl.csms[0].log2FoldChange;
                xlSr.pValue = xl.csms[0].pValue;

                if (xl.csms.Count > 1)
                {
                    int qtdFoldChange = xl.csms[0].log2FoldChange.Count;
                    xlSr.log2FoldChange = new();
                    xlSr.pValue = new();
                    for (int i = 0; i < qtdFoldChange; i++)
                    {
                        var folds = xl.csms.Select(a => a.log2FoldChange[i]).ToList();

                        double median = 0;
                        if (folds.Count > 0)
                        {
                            if (folds.Count == 1) 
                                median = folds[0];
                            else
                            {
                                var orderedQuant = folds.OrderBy(p => p);
                                int count = orderedQuant.Count();
                                median = orderedQuant.ElementAt(count / 2) + orderedQuant.ElementAt((count - 1) / 2);
                                median /= 2;
                            }
                        }

                        xlSr.log2FoldChange.Add(median);
                        xlSr.pValue.Add(folds.Count > 1 ? IsobaricUtils.computeOneSampleTtest(folds) : xl.csms[0].pValue[i]);
                    }
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

        /// <summary>
        /// Method responsible for quantifying CSMs
        /// </summary>
        private void computeCSMQuant()
        {
            if (resultsPackage == null || resultsPackage.CSMSearchResults == null)
                throw new Exception("There is no spectra to be quantified.");

            Console.WriteLine("Performing quantitation taking into account " + resultsPackage.CSMSearchResults.Count + " scans.");

            foreach (CSMSearchResult csm in resultsPackage.CSMSearchResults)
            {
                string fileName = resultsPackage.FileNameIndex[csm.fileIndex];

                if (csm.quantitation == null) continue;

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
                    csm.avg_notNull = new();
                    List<int> uniqueClasses = myParams.ClassLabels.Distinct().ToList();
                    for (int i = 1; i <= uniqueClasses.Count; i++)
                        csm.avg_notNull.Add(IsobaricUtils.computeAVG(csm.quantitation, i, myParams.ClassLabels));

                    csm.log2FoldChange = new();
                    csm.pValue = new();
                    for (int i = 0; i < uniqueClasses.Count; i++)
                    {
                        if (csm.avg_notNull.Count > i + 1)
                        {
                            if (csm.avg_notNull[i + 1] > 0)
                            {
                                csm.log2FoldChange.Add(Math.Log2(csm.avg_notNull[0] / csm.avg_notNull[i + 1]));
                                csm.pValue.Add(double.IsNaN(csm.log2FoldChange[0]) ? double.NaN : IsobaricUtils.computeTtest(csm.quantitation));
                            }
                            else
                            {
                                csm.log2FoldChange.Add(0);
                                csm.pValue.Add(0);
                            }
                        }
                        else break;
                    }
                }
            }

            Console.WriteLine("Done!");
        }

        private void MultinochMS2(List<MSUltraLight> ms2pectraFromAThermoFile)
        {
            Console.Write("Multinoch MS2-MS2 spectra ... ");

            List<int> ms2ChimeraSpectra = new();
            int object_processed = 0;
            int old_progress = 0;
            double totalObjects = ms2pectraFromAThermoFile.Count;

            Parallel.ForEach(ms2pectraFromAThermoFile,
                  new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                  ms2Chimera =>

                  //foreach (MSUltraLight ms2Chimera in ms2pectraFromAThermoFile)
                  {
                      if (!ms2ChimeraSpectra.Contains(ms2Chimera.ScanNumber))
                      {
                          MSUltraLight ms2 = ms2pectraFromAThermoFile.Where(a => a.PrecursorScanNumber == ms2Chimera.ScanNumber).FirstOrDefault();

                          if (ms2 != null)
                          {
                              ms2ChimeraSpectra.Add(ms2.ScanNumber);

                              ms2Chimera.Ions.AddRange(ms2.Ions);
                              ms2Chimera.Ions = ms2.Ions.Distinct().ToList();
                              ms2Chimera.Ions.Sort();
                          }
                      }

                      object_processed++;
                      int new_progress = (int)((double)object_processed / (totalObjects) * 100);
                      if (new_progress > old_progress)
                      {
                          old_progress = new_progress;
                          Console.Write("Processing MS2-MS2 spectra: " + old_progress + "%");
                      }
                  }
            );
        }

        /// <summary>
        /// Method responsible for merging ms3 and ms2 spectra
        /// </summary>
        /// <param name="rawFile"></param>
        /// <param name="ms2pectraFromAThermoFile"></param>
        private void MultinochSPSMS3(FileInfo rawFile, List<MSUltraLight> ms2pectraFromAThermoFile)
        {
            string current_fileNme = rawFile.Name.Substring(0, rawFile.Name.Length - rawFile.Extension.Length);
            int rawFileIndex = resultsPackage.FileNameIndex.IndexOf(current_fileNme);

            Console.Write("Multinoch SPS-MS3 spectra ... ");
            List<PatternTools.MSParserLight.MSUltraLight> ms3pectraFromAThermoFile = PatternTools.MSParserLight.ParserUltraLightRawFlash.Parse(rawFile.FullName, 3, (short)rawFileIndex, false, null, stdOut_console, 400).ToList();

            if (ms3pectraFromAThermoFile.Count == 0) return;


            int object_processed = 0;
            int old_progress = 0;
            double totalObjects = ms3pectraFromAThermoFile.Count;

            Parallel.ForEach(ms3pectraFromAThermoFile,
                  new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                  ms3 =>

                  //foreach (PatternTools.MSParserLight.MSUltraLight ms3 in ms3pectraFromAThermoFile)
                  {
                      MSUltraLight ms2 = ms2pectraFromAThermoFile.Where(a => a.ScanNumber == ms3.PrecursorScanNumber).FirstOrDefault();
                      if (ms2 != null)
                      {
                          ms2.Ions.AddRange(from ion in ms3.Ions.AsParallel()
                                            select Tuple.Create(ion.MZ, ion.Intensity));
                          ms2.Ions = ms2.Ions.Distinct().ToList();
                          ms2.Ions.Sort();
                      }
                      object_processed++;
                      int new_progress = (int)((double)object_processed / (totalObjects) * 100);
                      if (new_progress > old_progress)
                      {
                          old_progress = new_progress;
                          Console.Write("Processing SPS-MS3 spectra: " + old_progress + "%");
                      }
                  }
            );
        }

        /// <summary>
        /// Method responsible for computing Multinoch (chimera) spectra
        /// </summary>
        /// <param name="rawFile"></param>
        /// <param name="ms2pectraFromAThermoFile"></param>
        private void MultiNoch(FileInfo rawFile, List<MSUltraLight> ms2pectraFromAThermoFile)
        {
            if (myParams.Multinoch == 1)//MS2-MS2
            {
                this.MultinochMS2(ms2pectraFromAThermoFile);
            }
            else
            {
                this.MultinochSPSMS3(rawFile, ms2pectraFromAThermoFile);
            }

        }

        /// <summary>
        /// Method responsible for reading all spectra files and process all of them
        /// </summary>
        private void processRawFiles()
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

            //MS1 Correction
            double isolationWindow = 0.7;
            double envelopePPMMS1 = 15;
            int maxCharge = 7;
            double stringency = 0.92;
            bool checkMultiplexSpectra = false;

            resultsPackage.Spectra = new List<MSUltraLight>();
            foreach (FileInfo rawFile in rawFiles)
            {
                Console.WriteLine("Extracting MS/MS for " + rawFile.Name);

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
                                                                 PrecursorScanNumber = ms.PrecursorScanNumber
                                                             }).ToList();
                spectraFromAThermoFile.RemoveAll(a => a.Ions == null);

                int object_processed = 0;
                int old_progress = 0;
                double totalObjects = 0;
                List<PatternTools.MSParserLight.MSUltraLight> ms1SpectraFromAThermoFile = null;
                List<EnvelopeScore>[] ms1Envelopes = null;
                if (checkMultiplexSpectra)
                {
                    #region Looking for multiplex spectra
                    Console.WriteLine();

                    Console.WriteLine("Detecting multiplex spectra...\n");

                    Console.WriteLine("Extracting MS for " + rawFile.Name);
                    ms1SpectraFromAThermoFile = PatternTools.MSParserLight.ParserUltraLightRawFlash.Parse(rawFile.FullName, 1, (short)rawFileIndex, false, null, stdOut_console, 400).ToList();

                    ms1Envelopes = new List<EnvelopeScore>[ms1SpectraFromAThermoFile.Count];
                    DeconvolutionSimple deconvoluterMS1 = new DeconvolutionSimple(envelopePPMMS1, maxCharge);

                    totalObjects = ms1SpectraFromAThermoFile.Count;
                    Parallel.For(0, ms1SpectraFromAThermoFile.Count,
                       index =>
                       {
                           (List<EnvelopeScore> theseEnvelopes, List<(double, double)> newIons) = deconvoluterMS1.DeconvoluteMS(stringency, ms1SpectraFromAThermoFile[index].Ions, false);
                           ms1Envelopes[index] = theseEnvelopes;
                           object_processed++;
                           int new_progress = (int)((double)object_processed / (totalObjects) * 100);
                           if (new_progress > old_progress)
                           {
                               old_progress = new_progress;
                               Console.Write("Detecting multiplex spectra: " + old_progress + "%");
                           }
                       });

                    #endregion
                }

                if (myParams.Multinoch > 0)//0: None; 1: MS2-MS2; 2: SPS-MS3
                {
                    this.MultiNoch(rawFile, spectraFromAThermoFile);
                }

                double[] totalSignal = new double[myParams.MarkerMZs.Count];

                Console.WriteLine("Computing CSM quantitation...");

                object progress_lock = new object();
                object_processed = 0;
                old_progress = 0;
                totalObjects = spectraFromAThermoFile.Count;

                int totalMultiplexSpectra = 0;
                foreach (MSUltraLight ms2 in spectraFromAThermoFile)
                {
                    if (checkMultiplexSpectra)//Checking multiplex spectra
                    {
                        //Find the closest ms1
                        List<int> distances = ms1SpectraFromAThermoFile.Select(a => Math.Abs(a.ScanNumber - ms2.ScanNumber)).ToList();
                        if (distances.Count == 0) continue;
                        int minDistance = distances.Min();
                        int indexOfMin = distances.IndexOf(minDistance);
                        List<EnvelopeScore> candiates = ms1Envelopes[indexOfMin];

                        candiates.RemoveAll(a => a.ChargeScoreList[0].Charge == 1);

                        //Find envelopes that have the most intense peak in the isolation window
                        //We include this 0.1 as things isolated in the limits of the isolation window are not properly isolated
                        double isolationMin = ms2.Precursors[0].Item1 - isolationWindow + 0.1;
                        double isolationMax = ms2.Precursors[0].Item1 + isolationWindow - 0.1;

                        int numberOfPrecursor = 0;
                        foreach (EnvelopeScore envelope in candiates)
                        {
                            ChargeScore cs = envelope.ChargeScoreList[0];
                            //Verify if an isotope with intensity above 0.1 is within the isolation window

                            for (float i = 0; i < cs.AcumulatedNormalizedSignal.Count; i++)
                            {
                                double signal = cs.AcumulatedNormalizedSignal[(int)i];

                                if (signal < 0.1) { continue; } //Lets make sure we are only dealing with stuff having a minimum intensity here

                                double mz = envelope.MZ + (i / (float)cs.Charge);

                                if (mz > isolationMin && mz < isolationMax)
                                {
                                    //Make sure we are not adding the same stuff
                                    if ((Math.Abs(ms2.Precursors[0].Item1 - mz) > 0.01) && ms2.Precursors[0].Item2 != cs.Charge)
                                    {
                                        numberOfPrecursor++;
                                        break;
                                    }
                                }
                            }
                        }

                        //Don't consider multiplex spectra
                        if (numberOfPrecursor > 0)
                        {
                            totalMultiplexSpectra++;
                            continue;
                        }
                    }

                    //Get info for total signal normalization
                    double[] thisQuantitation = GetIsobaricSignal(ms2.Ions.Where(a => a.Item1 < 200).ToList(), myParams.MarkerMZs);
                    double maxSignal = thisQuantitation.Max();

                    //We can only correct for signal for those that have quantitation values in all places    
                    if (myParams.NormalizationPurityCorrection && (thisQuantitation.Count(a => a > 0) == myParams.MarkerMZs.Count))
                    {
                        thisQuantitation = IsobaricImpurityCorrection.CorrectForSignal(purityCorrectionsMatrix, thisQuantitation).ToArray();
                    }

                    // If a signal is less than the percentage specified in the ion threshold it should become 0.  
                    for (int i = 0; i < thisQuantitation.Length; i++)
                    {
                        if (thisQuantitation[i] < maxSignal * myParams.IonThreshold)
                        {
                            thisQuantitation[i] = 0;
                        }
                    }

                    for (int i = 0; i < thisQuantitation.Length; i++)
                    {
                        totalSignal[i] += thisQuantitation[i];
                    }

                    if (rawFileIndex != -1)
                    {
                        List<CSMSearchResult> cSMSearchResults = resultsPackage.CSMSearchResults.Where(a => a.scanNumber == ms2.ScanNumber && a.fileIndex == rawFileIndex).ToList();
                        if (cSMSearchResults != null && cSMSearchResults.Count > 0)
                        {
                            if (cSMSearchResults.Count == 1)
                            {
                                cSMSearchResults[0].quantitation = thisQuantitation.ToList();
                            }
                            else
                            {
                                foreach (CSMSearchResult csmSr in cSMSearchResults)
                                {
                                    csmSr.quantitation = thisQuantitation.ToList();
                                }
                            }
                            resultsPackage.Spectra.Add(ms2);
                        }
                    }

                    lock (progress_lock)
                    {
                        object_processed++;
                        int new_progress = (int)((double)object_processed / (totalObjects) * 100);
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

                if (checkMultiplexSpectra)
                    Console.Write("Total multiplex spectra: " + totalMultiplexSpectra);
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
