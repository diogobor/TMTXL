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
using TMTXL.Control;
using TMTXL.Model;

namespace TMTXL.Results
{
    /// <summary>
    /// Interaction logic for ResultsWin.xaml
    /// </summary>
    public partial class ResultsWin : Window
    {
        private ResultsPackage MyResults;

        public ResultsWin()
        {
            InitializeComponent();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void Setup(ResultsPackage myResults)
        {
            MyResults = myResults;

            ppi_results_datagrid.ItemsSource = (from ppi in MyResults.PPIResults.AsParallel()
                                               select new
                                               {
                                                   gene_A = ppi.gene_a,
                                                   gene_B = ppi.gene_b,
                                                   protein_A = ppi.protein_a,
                                                   protein_B = ppi.protein_b,
                                                   score = ppi.score,
                                                   channel_126 = ppi.quantitation[0],
                                                   channel_127N = ppi.quantitation[1],
                                                   channel_127C = ppi.quantitation[2],
                                                   channel_128N = ppi.quantitation[3],
                                                   channel_128C = ppi.quantitation[4],
                                                   channel_129N = ppi.quantitation[5],
                                                   channel_129C = ppi.quantitation[6],
                                                   channel_130N = ppi.quantitation[7],
                                                   channel_130C = ppi.quantitation[8],
                                                   channel_131 = ppi.quantitation[9]
                                               }
                                            ).ToList().OrderBy(a => a.gene_A);

            xl_results_datagrid.ItemsSource = (from xl in MyResults.XLSearchResults.AsParallel()
                                               select new
                                               {
                                                   alpha_peptide = xl.peptide_alpha,
                                                   beta_peptide = xl.peptide_beta,
                                                   pos_alpha = xl.pos_alpha,
                                                   pos_beta = xl.pos_beta,
                                                   channel_126 = xl.quantitation[0],
                                                   channel_127N = xl.quantitation[1],
                                                   channel_127C = xl.quantitation[2],
                                                   channel_128N = xl.quantitation[3],
                                                   channel_128C = xl.quantitation[4],
                                                   channel_129N = xl.quantitation[5],
                                                   channel_129C = xl.quantitation[6],
                                                   channel_130N = xl.quantitation[7],
                                                   channel_130C = xl.quantitation[8],
                                                   channel_131 = xl.quantitation[9]
                                               }
                                            ).ToList().OrderBy(a => a.alpha_peptide);

            csm_results_datagrid.ItemsSource = (from csm in MyResults.CSMSearchResults.AsParallel()
                                                select new
                                                {
                                                    scanNumber = csm.scanNumber,
                                                    file_name = MyResults.FileNameIndex[csm.fileIndex] + ".raw",
                                                    alpha_peptide = csm.peptide_alpha,
                                                    beta_peptide = csm.peptide_beta,
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

        private void csm_results_datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int scanNumber = (int)csm_results_datagrid.SelectedItem.GetType().GetProperty("scanNumber").GetValue(csm_results_datagrid.SelectedItem, null);

            CSMSearchResult csm = MyResults.CSMSearchResults.Where(a => a.scanNumber == scanNumber).FirstOrDefault();

            MSUltraLight ms = MyResults.Spectra.Where(a => a.ScanNumber == csm.scanNumber && a.FileNameIndex == csm.fileIndex).FirstOrDefault();

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
            svf.PlotSpectrum(ions, ppm, "", PatternTools.PTMMods.DefaultModifications.TheModifications, false, IonA, IonB, IonC, IonX, IonY, IonZ, ms.Precursors[0].Item2);
            svf.ShowDialog();
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            if (MyResults == null || MyResults.CSMSearchResults == null || MyResults.CSMSearchResults.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("There is no data to be processed!", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.Filter = "TMTXL results (*.tmtxl)|*.tmtxl"; // Filter files by extension
            dlg.Title = "Save results";

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string FileName = dlg.FileName;

                try
                {
                    MyResults.SerializeResults(FileName);
                    System.Windows.Forms.MessageBox.Show("The results have been saved successfully!", "Information", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("ERROR: " + exc.Message);
                    System.Windows.Forms.MessageBox.Show("Failed to save!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
        }

        private void MenuItemLoad_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.Filter = "TMTXL results (*.tmtxl)|*.tmtxl"; // Filter files by extension
            dlg.Title = "Load results";

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                try
                {
                    MyResults = MyResults.DeserializeResults(dlg.FileName);
                    this.Setup(MyResults);
                    System.Windows.Forms.MessageBox.Show("The results have been load successfully!", "Information", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("ERROR: " + exc.Message);
                    System.Windows.Forms.MessageBox.Show("Failed to load!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
        }
    }
}
