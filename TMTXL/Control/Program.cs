using IsobaricAnalyzer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using TMTXL.Model;
using TMTXL.Parser;

namespace TMTXL.Control
{
    public class Program
    {
        public ProgramParams programParams;
        public bool FinishProcessing;
        public bool ErrorProcessing;
        public string FinalTime { get; set; }

        public ResultsPackage resultsPackage { get; set; }

        [STAThread]
        public static void Main()
        {
            #region Setting Language
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            if (!Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\International", "LocaleName", null).ToString().ToLower().Equals("en-us"))
            {
                DialogResult answer = System.Windows.Forms.MessageBox.Show("The default language is not English. Do you want to change it to English ?\nThis tool works if only the default language is English.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (answer == DialogResult.Yes)
                {
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "Locale", "00000409");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "LocaleName", "en-US");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sCountry", "Estados Unidos");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sCurrency", "$");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sDate", "/");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sDecimal", ".");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sGrouping", "3;0");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sLanguage", "ENU");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sList", ",");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sLongDate", "dddd, MMMM dd, yyyy");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sMonDecimalSep", ".");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sMonGrouping", "3;0");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sMonThousandSep", ",");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sNativeDigits", "0123456789");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sNegativeSign", "-");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sPositiveSign", "");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sShortDate", "M/d/yyyy");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sThousand", ",");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sTime", ":");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sTimeFormat", "h:mm:ss tt");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sShortTime", "h:mm tt");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sYearMonth", "MMMM, yyyy");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iCalendarType", "1");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iCountry", "1");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iCurrDigits", "2");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iCurrency", "0");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iDate", "0");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iDigits", "2");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "NumShape", "1");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iFirstDayOfWeek", "6");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iFirstWeekOfYear", "0");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iLZero", "1");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iMeasure", "1");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iNegCurr", "0");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iNegNumber", "1");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iPaperSize", "1");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iTime", "0");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iTimePrefix", "0");
                    Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iTLZero", "0");
                    System.Windows.Forms.MessageBox.Show("Software will be restarted!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    System.Environment.Exit(0);
                    System.Windows.Forms.Application.Exit();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Software will be closed!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    System.Environment.Exit(0);
                    System.Windows.Forms.Application.Exit();
                }
            }
            #endregion

            App application = new App();
            application.InitializeComponent();
            application.Run();
        }

        public void Run()
        {
            FinishProcessing = false;
            ErrorProcessing = false;

            DateTime beginTimeSearch = DateTime.Now;
            string version = "";
            try
            {
                version = Assembly.GetExecutingAssembly().GetName()?.Version.ToString();
            }
            catch (Exception e1)
            {
                //Unable to retrieve version number
                Console.WriteLine("", e1);
                version = "";
            }

            Console.WriteLine("#################################################################");
            Console.WriteLine("                                                   TMT - XLMS - v. " + version);
            Console.WriteLine("                                                Engineered by The Liu Lab             ");
            Console.WriteLine("#################################################################");

            List<FileInfo> xlinkCSMFiles = null;
            List<FileInfo> xlinkPPIFiles = null;

            try
            {
                DirectoryInfo folder = new DirectoryInfo(programParams.IDdir);
                xlinkCSMFiles = folder.GetFiles("*_tm.csv", SearchOption.AllDirectories).ToList();
                xlinkPPIFiles = folder.GetFiles("*.csv", SearchOption.AllDirectories).ToList();

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(
                        "Error reading files",
                        "Warning",
                        (System.Windows.MessageBoxButton)MessageBoxButtons.OK,
                        (System.Windows.MessageBoxImage)MessageBoxIcon.Warning);
                Console.WriteLine("ERROR: " + e.Message);
                ErrorProcessing = true;
                return;
            }

            resultsPackage = new ResultsPackage();
            resultsPackage.Params = programParams;

            for (int i = 0; i < xlinkCSMFiles.Count; i++)
            {
                try
                {
                    Console.WriteLine("Reading XlinkX CSM data: " + xlinkCSMFiles[i].FullName);
                    //Get RAW fie name
                    string[] cols = xlinkCSMFiles[i].DirectoryName.Split("\\");
                    resultsPackage.FileNameIndex.Add(cols[cols.Length - 1]);
                    resultsPackage.CSMSearchResults.AddRange(ParserXlinkX.ParseCSMs(xlinkCSMFiles[i].FullName, (short)i));
                }
                catch (Exception ex)
                {
                    ErrorProcessing = true;
                }
            }

            for (int i = 0; i < xlinkPPIFiles.Count; i++)
            {
                try
                {
                    resultsPackage.PPIResults.AddRange(ParserXlinkX.ParsePPI(xlinkPPIFiles[i].FullName));
                    ErrorProcessing = false;
                }
                catch (Exception ex)
                {
                    ErrorProcessing = true;
                }
            }

            IsobaricAnalyzerControl isobaricAnalyzerControl = new IsobaricAnalyzerControl();
            isobaricAnalyzerControl.stdOut_console = false;
            isobaricAnalyzerControl.resultsPackage = resultsPackage;

            #region set params
            isobaricAnalyzerControl.myParams = setIsobaricAnalyzerParams();
            #endregion

            try
            {
                #region purity correction matrix

                if (isobaricAnalyzerControl.myParams.NormalizationPurityCorrection)
                {
                    Console.WriteLine(" Retrieving purity correction matrix...");
                    isobaricAnalyzerControl.setPurityCorrectionsMatrix(programParams.PurityCorrectionMatrix);
                }
                #endregion

                isobaricAnalyzerControl.computeQuantitation();
            }
            catch (Exception exc)
            {
                ErrorProcessing = true;
                Console.WriteLine("ERROR: " + exc.Message);
                System.Windows.MessageBox.Show(
                            exc.Message,
                            "Error",
                            (MessageBoxButton)MessageBoxButtons.OK,
                            (MessageBoxImage)MessageBoxIcon.Error);
                return;
            }

            TimeSpan dt = DateTime.Now - beginTimeSearch;
            Console.WriteLine(" Idle : Processing time: {0} Day(s) {1} Hour(s) {2} Minute(s) {3} Second(s)", dt.Days, dt.Hours, dt.Minutes, dt.Seconds);

            Console.WriteLine(" ");
            FinalTime = dt.Days + " Day(s) " + dt.Hours + " Hour(s) " + dt.Minutes + " Minute(s) " + dt.Seconds + " Second(s).";

            FinishProcessing = true;
        }

        public IsobaricParams setIsobaricAnalyzerParams()
        {
            //Obtain class labels
            List<int> classLabels = Regex.Split(programParams.ClassLabels, " ").Select(a => int.Parse(a)).ToList();
            //Obtain the isobaric masses

            IsobaricParams ip = new IsobaricParams()
            {
                ClassLabels = classLabels,
                MarkerMZs = programParams.IsobaricMassess,
                AnalysisType = true,//It is not PLP file
                RAWDirectory = programParams.RawFilesDir,
                MarkerPPMTolerance = 20,
                IonThreshold = 0.025,
                NormalizationIdentifiedSpectra = false,
                NormalizationAllSpectra = true,
                NormalizationPurityCorrection = true,
                PatternLabProjectOnlyUniquePeptides = false,
                YadaMultiplexCorrectionDir = string.Empty,
                SPSMS3 = programParams.SPSMS3
            };

            return ip;
        }

    }
}
