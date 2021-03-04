using PatternTools.MSParserLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TMTXL.Model;

namespace TMTXL.Results
{
    /// <summary>
    /// Interaction logic for ResultsWin.xaml
    /// </summary>
    public partial class ResultsWin : Window
    {
        private List<CSMSearchResult> CSMSearchResults;
        private List<MSUltraLight> Spectra;

        public ResultsWin()
        {
            InitializeComponent();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void Setup(List<CSMSearchResult> cSMSearchResults, List<MSUltraLight> spectra)
        {
            Spectra = spectra;
            CSMSearchResults = cSMSearchResults;
            results_datagrid.ItemsSource = (from csm in cSMSearchResults.AsParallel()
                                            select new
                                            {
                                                scanNumber = csm.scanNumber,
                                                peptide_alpha = csm.peptide_alpha,
                                                peptide_beta = csm.peptide_beta,
                                                channel_126 = csm.quantitation[0],
                                                channel_127N = csm.quantitation[1],
                                                channel_127C = csm.quantitation[2],
                                                channel_128N = csm.quantitation[3],
                                                channel_128C = csm.quantitation[4],
                                                channel_129N = csm.quantitation[5],
                                                channel_129C = csm.quantitation[6],
                                                channel_130N = csm.quantitation[7],
                                                channel_130C = csm.quantitation[8],
                                                channel_131 = csm.quantitation[9]
                                            }
                                            ).ToList().OrderBy(a => a.scanNumber);


        }

        private void results_datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int scanNumber = (int)results_datagrid.SelectedItem.GetType().GetProperty("scanNumber").GetValue(results_datagrid.SelectedItem, null);

            CSMSearchResult csm = CSMSearchResults.Where(a => a.scanNumber == scanNumber).FirstOrDefault();

            MSUltraLight ms = Spectra.Where(a => a.ScanNumber == csm.scanNumber && a.FileNameIndex == csm.fileIndex).FirstOrDefault();

            if (ms == null) return;
            
            List<Tuple<float, float>> ions = (from peak in ms.Ions
                                              select System.Tuple.Create((float)peak.Item1, (float)peak.Item2)).ToList();


            #region Get Activation Type
            bool IonA = true;
            bool IonB = true;
            bool IonC = false;
            bool IonX = false;
            bool IonY = true;
            bool IonZ = false;

            #endregion

            SpectrumViewer2.PeprideSpectrumViewer.MSViewerWindow svf = new SpectrumViewer2.PeprideSpectrumViewer.MSViewerWindow();
            double ppm = 450;
            svf.PlotSpectrum(ions, ppm, "", PatternTools.PTMMods.DefaultModifications.TheModifications, false, IonA, IonB, IonC, IonX, IonY, IonZ, ms.Precursors[0].Z);
            svf.ShowDialog();
        }
    }
}
