using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader;
using TMTXL.Model;

namespace TMTXL.Parser
{
    
    public class ParserThermo
    {
        public static int Progress { get; private set; }

        public static List<MassSpectrum> Parse(string rawFileName, short MsnLevel = 2, short fileIndex = -1, bool saveScanHeader = false)
        {
            List<MassSpectrum> tmpList = new List<MassSpectrum>();

            object progress_lock = new object();
            int old_progress = 0;

            int maximumNumberOfPeaks = 1000;
            double relativeThresholdPercent = 0.01;

            if (File.Exists(rawFileName))
            {
                Console.Write("Reading RAW File: " + old_progress + "%");

                try
                {
                    IRawDataPlus rawFile = RawFileReaderAdapter.FileFactory(rawFileName);
                    if (!rawFile.IsOpen || rawFile.IsError)
                    {
                        Console.Write(" Error: unable to access the RAW file using the RawFileReader class.");
                        return tmpList;
                    }

                    rawFile.SelectInstrument(Device.MS, 1);

                    // Get the first and last scan from the RAW file
                    int iFirstScan = rawFile.RunHeaderEx.FirstSpectrum;
                    int iLastScan = rawFile.RunHeaderEx.LastSpectrum;

                    int iNumPeaks = -1;
                    short iPrecursorCharge = -1;
                    double dPrecursor = -1;
                    short iMassAnalyzer = -1;
                    short iActivationType = -1;
                    double dPrecursorMZ = -1;
                    double[] pdMass;
                    double[] pdInten;


                    for (int iScanNumber = iFirstScan; iScanNumber <= iLastScan; iScanNumber++)
                    {

                        ScanStatistics scanStatistics = rawFile.GetScanStatsForScanNumber(iScanNumber);
                        string sScanHeader = scanStatistics.ScanType;
                        double dRT = rawFile.RetentionTimeFromScanNumber(iScanNumber);

                        // Get the scan filter for this scan number
                        IScanFilter scanFilter = rawFile.GetFilterForScanNumber(iScanNumber);

                        if (!string.IsNullOrEmpty(scanFilter.ToString()) && MsnLevel == -1 ||
                        (scanFilter.MSOrder == MSOrderType.Ms && MsnLevel == 1 ||
                        scanFilter.MSOrder == MSOrderType.Ms2 && MsnLevel == 2 ||
                        scanFilter.MSOrder == MSOrderType.Ms2 && MsnLevel == 3))
                        {

                            // Check to see if the scan has centroid data or profile data.  Depending upon the
                            // type of data, different methods will be used to read the data.
                            CentroidStream centroidStream = rawFile.GetCentroidStream(iScanNumber, false);

                            if (centroidStream.Length > 0)
                            {
                                // Get the centroid (label) data from the RAW file for this scan

                                iNumPeaks = centroidStream.Length;
                                pdMass = new double[iNumPeaks];   // stores mass of spectral peaks
                                pdInten = new double[iNumPeaks];  // stores inten of spectral peaks
                                pdMass = centroidStream.Masses;
                                pdInten = centroidStream.Intensities;

                            }
                            else
                            {
                                // Get the segmented (low res and profile) scan data
                                SegmentedScan segmentedScan = rawFile.GetSegmentedScanFromScanNumber(iScanNumber, scanStatistics);
                                iNumPeaks = segmentedScan.Positions.Length;
                                pdMass = new double[iNumPeaks];   // stores mass of spectral peaks
                                pdInten = new double[iNumPeaks];  // stores inten of spectral peaks
                                pdMass = segmentedScan.Positions;
                                pdInten = segmentedScan.Intensities;
                            }

                            if (iNumPeaks > 0)
                            {

                                MassAnalyzerType massAnalyzer = rawFile.GetScanEventForScanNumber(iScanNumber).MassAnalyzer;

                                switch (massAnalyzer)
                                {
                                    case MassAnalyzerType.MassAnalyzerFTMS:
                                        iMassAnalyzer = 1;
                                        break;
                                    case MassAnalyzerType.MassAnalyzerITMS:
                                        iMassAnalyzer = 2;
                                        break;
                                    case MassAnalyzerType.MassAnalyzerTOFMS:
                                        iMassAnalyzer = 3;
                                        break;
                                    case MassAnalyzerType.MassAnalyzerSQMS:
                                        iMassAnalyzer = 4;
                                        break;
                                    default:
                                        iMassAnalyzer = -1;
                                        break;

                                }

                                // Get the scan event for this scan number
                                if (scanFilter.MSOrder != MSOrderType.Ms)
                                {
                                    IScanEvent scanEvent = rawFile.GetScanEventForScanNumber(iScanNumber);

                                    dPrecursor = scanEvent.GetReaction(0).PrecursorMass;

                                    ActivationType activationType = scanEvent.GetReaction(0).ActivationType;

                                    switch (activationType)
                                    {
                                        case ActivationType.CollisionInducedDissociation:
                                            iActivationType = 1;
                                            break;
                                        case ActivationType.HigherEnergyCollisionalDissociation:
                                            iActivationType = 2;
                                            break;
                                        case ActivationType.ElectronTransferDissociation:
                                            iActivationType = 3;
                                            break;
                                        case ActivationType.ElectronCaptureDissociation:
                                            iActivationType = 4;
                                            break;
                                        case ActivationType.MultiPhotonDissociation:
                                            iActivationType = 5;
                                            break;
                                        case ActivationType.Any:
                                            iActivationType = 6;
                                            break;
                                        case ActivationType.PQD:
                                            iActivationType = 7;
                                            break;
                                        default:
                                            iActivationType = -1;
                                            break;

                                    }

                                    LogEntry trailerData = rawFile.GetTrailerExtraInformation(iScanNumber);
                                    for (int i = 0; i < trailerData.Length; i++)
                                    {

                                        if (trailerData.Labels[i] == "Monoisotopic M/Z:")
                                            dPrecursorMZ = double.Parse(trailerData.Values[i]);
                                        else if (trailerData.Labels[i] == "Charge State:")
                                            iPrecursorCharge = (short)double.Parse(trailerData.Values[i]);

                                        //switch (trailerData.Labels[i])
                                        //{
                                        //    case "Monoisotopic M/Z:":
                                        //        dPrecursorMZ = double.Parse(trailerData.Values[i]);
                                        //        break;
                                        //    case "Charge State:":
                                        //        iPrecursorCharge = (short)double.Parse(trailerData.Values[i]);
                                        //        break;
                                        //    case "Access ID:":
                                        //        iAccessID = (short)double.Parse(trailerData.Values[i]);
                                        //        break;
                                        //    case "SPS Masses:":
                                        //        dSPSMasses = (short)double.Parse(trailerData.Values[i].Split(",")[0]);
                                        //        break;
                                        //    default:
                                        //        break;
                                        //}
                                    }

                                    dPrecursorMZ = dPrecursorMZ == 0 ? dPrecursor : dPrecursorMZ;

                                }

                                //double dPepMass = (dPrecursorMZ * iPrecursorCharge) - (iPrecursorCharge - 1) * 1.00727646688;

                                (double MZ, double Intensity)[] ions = new (double MZ, double Intensity)[pdMass.Length];

                                for (int i = 0; i < pdInten.Length; i++)
                                {
                                    ions[i].MZ = pdMass[i];
                                    ions[i].Intensity = pdInten[i];

                                }

                                double relative_threshold = ions.Max(a => a.Intensity) * (relativeThresholdPercent / 100.0);

                                ions = ions.OrderByDescending(a => a.Intensity).Take(maximumNumberOfPeaks).Where(a => a.Intensity > relative_threshold).OrderBy(a => a.MZ).ToArray();

                                MassSpectrum ms = new MassSpectrum()
                                {
                                    ActivationType = iActivationType,
                                    ChromatographyRetentionTime = dRT,
                                    FileNameIndex = fileIndex,
                                    InstrumentType = iMassAnalyzer,
                                    Ions = ions.ToList(),
                                    MSLevel = (short)scanFilter.MSOrder,
                                    Precursors = new List<(double MZ, short Z)>() { (dPrecursorMZ, iPrecursorCharge) },
                                    ScanNumber = iScanNumber,
                                };

                                if (saveScanHeader)
                                    ms.ScanHeader = sScanHeader;

                                tmpList.Add(ms);

                                lock (progress_lock)
                                {
                                    Progress = (int)((double)iScanNumber / (iLastScan - iFirstScan) * 100);
                                    if (Progress > old_progress)
                                    {
                                        old_progress = Progress;
                                        //int currentLineCursor = Console.CursorTop;
                                        //Console.SetCursorPosition(0, Console.CursorTop);
                                        Console.Write("Reading RAW File: " + old_progress + "%");
                                        //Console.SetCursorPosition(0, currentLineCursor);
                                    }

                                }

                            }
                        }

                    }

                    rawFile.Dispose();
                }
                catch (Exception rawSearchEx)
                {
                    Console.WriteLine(" Error: " + rawSearchEx.Message);
                }
            }
            else
            {
                Console.WriteLine("No raw file has been found.");
            }

            Console.Write("Reading RAW File: 100%");

            return tmpList;

        }
    }
}
