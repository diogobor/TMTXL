using System;
using System.Collections.Generic;
using System.Data;
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

        /// <summary>
        /// Method responsible for crete data table for CSMs
        /// </summary>
        /// <returns>data table</returns>
        private DataTable createDataTableCSM()
        {
            DataTable dtCSM = new DataTable();

            dtCSM.Columns.Add("Scan Number", typeof(int));
            dtCSM.Columns.Add("File Name");
            dtCSM.Columns.Add("Gene A");
            dtCSM.Columns.Add("Gene B");
            dtCSM.Columns.Add("Alpha Peptide");
            dtCSM.Columns.Add("Alpha position", typeof(int));
            dtCSM.Columns.Add("Beta position", typeof(int));
            dtCSM.Columns.Add("Beta Peptide");
            dtCSM.Columns.Add("126", typeof(double));
            dtCSM.Columns.Add("127N", typeof(double));
            dtCSM.Columns.Add("127C", typeof(double));
            dtCSM.Columns.Add("128N", typeof(double));
            dtCSM.Columns.Add("128C", typeof(double));
            dtCSM.Columns.Add("129N", typeof(double));
            dtCSM.Columns.Add("129C", typeof(double));
            dtCSM.Columns.Add("130N", typeof(double));
            dtCSM.Columns.Add("130C", typeof(double));
            dtCSM.Columns.Add("131", typeof(double));
            dtCSM.Columns.Add("Avg 1 not Null", typeof(double));
            dtCSM.Columns.Add("Avg 2 not Null", typeof(double));
            dtCSM.Columns.Add("Log2(Fold Change)", typeof(double));
            dtCSM.Columns.Add("p-value", typeof(double));

            foreach (CSMSearchResult csm in MyResults.CSMSearchResults)
            {
                var row = dtCSM.NewRow();
                row["Scan Number"] = csm.scanNumber;
                row["File Name"] = MyResults.FileNameIndex[csm.fileIndex] + ".raw";
                row["Gene A"] = String.Join(" ,", csm.genes_alpha);
                row["Gene B"] = String.Join(" ,", csm.genes_beta);
                row["Alpha Peptide"] = csm.alpha_peptide;
                row["Beta Peptide"] = csm.beta_peptide;
                row["Alpha position"] = csm.alpha_pos_xl;
                row["Beta position"] = csm.beta_pos_xl;
                if (true)//TMT
                {
                    row["126"] = csm.quantitation[0];
                    row["127N"] = csm.quantitation[1];
                    row["127C"] = csm.quantitation[2];
                    row["128N"] = csm.quantitation[3];
                    row["128C"] = csm.quantitation[4];
                    row["129N"] = csm.quantitation[5];
                    row["129C"] = csm.quantitation[6];
                    row["130N"] = csm.quantitation[7];
                    row["130C"] = csm.quantitation[8];
                    row["131"] = csm.quantitation[9];
                }
                row["Avg 1 not Null"] = csm.avg_notNull_1;
                row["Avg 2 not Null"] = csm.avg_notNull_2;
                row["Log2(Fold Change)"] = csm.log2FoldChange;
                row["p-value"] = csm.pValue;
                dtCSM.Rows.Add(row);
            }

            return dtCSM;
        }

        /// <summary>
        /// Method responsible for creating data table for XLs
        /// </summary>
        /// <param name="dtCSM"></param>
        /// <returns></returns>
        private DataTable createDataTableXL()
        {
            DataTable dtXL = new DataTable();

            dtXL.Columns.Add("Alpha Peptide");
            dtXL.Columns.Add("Beta Peptide");
            dtXL.Columns.Add("Alpha position", typeof(int));
            dtXL.Columns.Add("Beta position", typeof(int));
            dtXL.Columns.Add("Spec count", typeof(int));
            dtXL.Columns.Add("Log2(Fold Change)", typeof(double));
            dtXL.Columns.Add("p-value", typeof(double));

            foreach (CSMSearchResult xl in MyResults.XLSearchResults)
            {
                var row = dtXL.NewRow();
                row["Alpha Peptide"] = xl.alpha_peptide;
                row["Beta Peptide"] = xl.beta_peptide;
                row["Alpha position"] = xl.alpha_pept_xl_pos;
                row["Beta position"] = xl.beta_pept_xl_pos;
                row["Spec count"] = xl.specCount;
                row["Log2(Fold Change)"] = xl.log2FoldChange;
                row["p-value"] = xl.pValue;
                dtXL.Rows.Add(row);
            }

            return dtXL;
        }

        /// <summary>
        /// Method responsible for creating data table for residues
        /// </summary>
        /// <param name="dtCSM"></param>
        /// <returns></returns>
        private DataTable createDataTableResidue()
        {
            DataTable dtResidues = new DataTable();

            dtResidues.Columns.Add("Gene A");
            dtResidues.Columns.Add("Gene B");
            dtResidues.Columns.Add("Alpha position", typeof(int));
            dtResidues.Columns.Add("Beta position", typeof(int));
            dtResidues.Columns.Add("Spec count", typeof(int));
            dtResidues.Columns.Add("Log2(Fold Change)", typeof(double));
            dtResidues.Columns.Add("p-value", typeof(double));

            foreach (CSMSearchResult xl in MyResults.ResidueSearchResults)
            {
                var row = dtResidues.NewRow();
                row["Gene A"] = xl.genes_alpha[0];
                row["Gene B"] = xl.genes_beta[0];
                row["Alpha position"] = xl.alpha_pept_xl_pos;
                row["Beta position"] = xl.beta_pept_xl_pos;
                row["Spec count"] = xl.specCount;
                row["Log2(Fold Change)"] = xl.log2FoldChange;
                row["p-value"] = xl.pValue;
                dtResidues.Rows.Add(row);
            }

            return dtResidues;
        }

        private DataTable createDataTablePPI()
        {
            DataTable dtPPI = new DataTable();

            dtPPI.Columns.Add("Gene A");
            dtPPI.Columns.Add("Gene B");
            dtPPI.Columns.Add("Protein A");
            dtPPI.Columns.Add("Protein B");
            dtPPI.Columns.Add("PPI score", typeof(double));
            dtPPI.Columns.Add("Spec count", typeof(int));
            dtPPI.Columns.Add("Log2(Fold Change)", typeof(double));
            dtPPI.Columns.Add("p-value", typeof(double));

            foreach (ProteinProteinInteraction ppi in MyResults.PPIResults)
            {
                var row = dtPPI.NewRow();
                row["Gene A"] = ppi.gene_a;
                row["Gene B"] = ppi.gene_b;
                row["Protein A"] = ppi.protein_a;
                row["Protein B"] = ppi.protein_b;
                row["PPI score"] = ppi.score;
                row["Spec count"] = ppi.specCount;
                row["Log2(Fold Change)"] = ppi.log2FoldChange;
                row["p-value"] = ppi.pValue;
                dtPPI.Rows.Add(row);
            }

            return dtPPI;
        }

        /// <summary>
        /// Method responsible for setting up 'ResultsPackage' object and initialize data grid views
        /// </summary>
        /// <param name="myResults"></param>
        public void Setup(ResultsPackage myResults)
        {
            MyResults = myResults;

            csm_results_datagrid.ItemsSource = createDataTableCSM().AsDataView();
            xl_results_datagrid.ItemsSource = createDataTableXL().AsDataView();
            residues_results_datagrid.ItemsSource = createDataTableResidue().AsDataView();
            ppi_results_datagrid.ItemsSource = createDataTablePPI().AsDataView();

        }

        private string GetSelectedValue(DataGrid grid)
        {
            DataGridCellInfo cellInfo = grid.SelectedCells[0];
            if (cellInfo == null) return "0";

            DataGridBoundColumn column = cellInfo.Column as DataGridBoundColumn;
            if (column == null) return null;

            FrameworkElement element = new FrameworkElement() { DataContext = cellInfo.Item };
            BindingOperations.SetBinding(element, TagProperty, column.Binding);

            return element.Tag.ToString();
        }

        private void csm_results_datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int scanNumber = Convert.ToInt32(GetSelectedValue(csm_results_datagrid));

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
