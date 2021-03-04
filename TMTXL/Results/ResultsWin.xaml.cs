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
        private List<MassSpectrum> MassSpectra;
        public ResultsWin()
        {
            InitializeComponent();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void Setup(List<MassSpectrum> tandemMassSpectra)
        {
            MassSpectra = tandemMassSpectra;
            results_datagrid.ItemsSource = (from tms in tandemMassSpectra.AsParallel()
                                            select new
                                            {
                                                scanNumber = tms.ScanNumber,
                                                mz = tms.Precursors[0].MZ,
                                                retentionTime = Math.Round(tms.ChromatographyRetentionTime, 3)
                                            }
                                            ).ToList().OrderBy(a => a.scanNumber);


        }

        private void results_datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int scanNumber = (int)results_datagrid.SelectedItem.GetType().GetProperty("scanNumber").GetValue(results_datagrid.SelectedItem, null);

            MassSpectrum ms = MassSpectra.Where(a => a.ScanNumber == scanNumber).FirstOrDefault();

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
