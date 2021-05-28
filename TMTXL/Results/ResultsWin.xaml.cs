using IsobaricAnalyzer;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using TMTXL.Utils;

namespace TMTXL.Results
{
    /// <summary>
    /// Interaction logic for ResultsWin.xaml
    /// </summary>
    public partial class ResultsWin : Window
    {

        private Thread saveThread;

        private IsobaricAnalyzerControl IsobaricAnalyzerControl;
        private ResultsPackage FilteredResults;

        private PlotController _ChartController;

        /// <summary>
        /// Method responsible for displaying dot property when mouse hovers over the point
        /// </summary>
        public PlotController ChartController
        {
            get
            {
                _ChartController = new OxyPlot.PlotController();
                _ChartController.Bind(new OxyPlot.OxyMouseEnterGesture(), OxyPlot.PlotCommands.HoverSnapTrack);
                return _ChartController;
            }
        }

        public ResultsWin()
        {
            InitializeComponent();

            this.DataContext = this;
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
            dtCSM.Columns.Add("α peptide");
            dtCSM.Columns.Add("β peptide");
            dtCSM.Columns.Add("α position", typeof(int));
            dtCSM.Columns.Add("β position", typeof(int));
            switch (FilteredResults.Params.ChemicalLabel)
            {
                case "iTRAQ 4":
                    dtCSM.Columns.Add("114", typeof(double));
                    dtCSM.Columns.Add("115", typeof(double));
                    dtCSM.Columns.Add("116", typeof(double));
                    dtCSM.Columns.Add("117", typeof(double));
                    break;
                case "TMT 6":
                    dtCSM.Columns.Add("126", typeof(double));
                    dtCSM.Columns.Add("127", typeof(double));
                    dtCSM.Columns.Add("128", typeof(double));
                    dtCSM.Columns.Add("129", typeof(double));
                    dtCSM.Columns.Add("130", typeof(double));
                    dtCSM.Columns.Add("131", typeof(double));
                    break;
                case "TMT 10":
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
                    break;
                case "TMT 16":
                    dtCSM.Columns.Add("126", typeof(double));
                    dtCSM.Columns.Add("127N", typeof(double));
                    dtCSM.Columns.Add("127C", typeof(double));
                    dtCSM.Columns.Add("128N", typeof(double));
                    dtCSM.Columns.Add("128C", typeof(double));
                    dtCSM.Columns.Add("129N", typeof(double));
                    dtCSM.Columns.Add("129C", typeof(double));
                    dtCSM.Columns.Add("130N", typeof(double));
                    dtCSM.Columns.Add("130C", typeof(double));
                    dtCSM.Columns.Add("131N", typeof(double));
                    dtCSM.Columns.Add("131C", typeof(double));
                    dtCSM.Columns.Add("132N", typeof(double));
                    dtCSM.Columns.Add("132C", typeof(double));
                    dtCSM.Columns.Add("133N", typeof(double));
                    dtCSM.Columns.Add("133C", typeof(double));
                    dtCSM.Columns.Add("134", typeof(double));
                    break;
            }

            var filteredCSMs = FilteredResults.CSMSearchResults;

            int qtdUniqueClass = Regex.Split(FilteredResults.Params.ClassLabels, " ").Distinct().Count();
            if (qtdUniqueClass > 1)
            {
                for (int i = 1; i <= qtdUniqueClass; i++)
                    dtCSM.Columns.Add("Avg " + i + " not Null", typeof(double));
                for (int i = 1; i <= qtdUniqueClass - 1; i++)
                    dtCSM.Columns.Add("Log2(Fold Change)" + i, typeof(double));
                for (int i = 1; i <= qtdUniqueClass - 1; i++)
                    dtCSM.Columns.Add("p-value" + i, typeof(double));

                FilteredResults.CSMSearchResults.OrderByDescending(a => a.log2FoldChange[0]).ThenByDescending(a => a.pValue[0]);
            }

            foreach (CSMSearchResult csm in filteredCSMs)
            {
                if (csm.quantitation == null) continue;

                var row = dtCSM.NewRow();
                row["Scan Number"] = csm.scanNumber;
                row["File Name"] = FilteredResults.FileNameIndex[csm.fileIndex] + ".raw";
                row["Gene A"] = String.Join(" ,", csm.genes_alpha);
                row["Gene B"] = String.Join(" ,", csm.genes_beta);
                row["α peptide"] = csm.alpha_peptide;
                row["β peptide"] = csm.beta_peptide;
                row["α position"] = csm.alpha_pos_xl;
                row["β position"] = csm.beta_pos_xl;

                switch (FilteredResults.Params.ChemicalLabel)
                {
                    case "iTRAQ 4":
                        row["114"] = csm.quantitation[0];
                        row["115"] = csm.quantitation[1];
                        row["116"] = csm.quantitation[2];
                        row["117"] = csm.quantitation[3];
                        break;
                    case "TMT 6":
                        row["126"] = csm.quantitation[0];
                        row["127"] = csm.quantitation[1];
                        row["128"] = csm.quantitation[2];
                        row["129"] = csm.quantitation[3];
                        row["130"] = csm.quantitation[4];
                        row["131"] = csm.quantitation[5];
                        break;
                    case "TMT 10":
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
                        break;
                    case "TMT 16":
                        row["126"] = csm.quantitation[0];
                        row["127N"] = csm.quantitation[1];
                        row["127C"] = csm.quantitation[2];
                        row["128N"] = csm.quantitation[3];
                        row["128C"] = csm.quantitation[4];
                        row["129N"] = csm.quantitation[5];
                        row["129C"] = csm.quantitation[6];
                        row["130N"] = csm.quantitation[7];
                        row["130C"] = csm.quantitation[8];
                        row["131N"] = csm.quantitation[9];
                        row["131C"] = csm.quantitation[10];
                        row["132N"] = csm.quantitation[11];
                        row["132C"] = csm.quantitation[12];
                        row["133N"] = csm.quantitation[13];
                        row["133C"] = csm.quantitation[14];
                        row["134"] = csm.quantitation[15];
                        break;
                }

                if (qtdUniqueClass > 1)
                {
                    for (int i = 1; i <= qtdUniqueClass; i++)
                        row["Avg " + i + " not Null"] = Utils.Utils.RoundUp(csm.avg_notNull[i - 1]);

                    for (int i = 1; i <= qtdUniqueClass - 1; i++)
                    {
                        row["Log2(Fold Change)" + i] = Math.Round(csm.log2FoldChange[i - 1], 4);
                        row["p-value" + i] = Math.Round(csm.pValue[i - 1], 4);
                    }
                }
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

            dtXL.Columns.Add("Gene A");
            dtXL.Columns.Add("Gene B");
            dtXL.Columns.Add("α peptide");
            dtXL.Columns.Add("β peptide");
            dtXL.Columns.Add("α position", typeof(int));
            dtXL.Columns.Add("β position", typeof(int));
            dtXL.Columns.Add("Spec count", typeof(int));

            var filteredXLs = FilteredResults.XLSearchResults;

            int qtdUniqueClass = Regex.Split(FilteredResults.Params.ClassLabels, " ").Distinct().Count();
            if (qtdUniqueClass > 1)
            {
                for (int i = 1; i <= qtdUniqueClass - 1; i++)
                    dtXL.Columns.Add("Log2(Fold Change)" + i, typeof(double));
                for (int i = 1; i <= qtdUniqueClass - 1; i++)
                    dtXL.Columns.Add("p-value" + i, typeof(double));

                filteredXLs = FilteredResults.XLSearchResults.OrderByDescending(a => a.log2FoldChange[0]).ThenByDescending(a => a.cSMs.Count).ToList();
            }

            foreach (XLSearchResult xl in filteredXLs)
            {
                var row = dtXL.NewRow();
                row["Gene A"] = String.Join(" ,", xl.cSMs[0].genes_alpha);
                row["Gene B"] = String.Join(" ,", xl.cSMs[0].genes_beta);
                row["α peptide"] = xl.cSMs[0].alpha_peptide;
                row["β peptide"] = xl.cSMs[0].beta_peptide;
                row["α position"] = xl.cSMs[0].alpha_pept_xl_pos;
                row["β position"] = xl.cSMs[0].beta_pept_xl_pos;
                row["Spec count"] = xl.cSMs.Count;

                if (qtdUniqueClass > 1)
                {
                    for (int i = 1; i <= qtdUniqueClass - 1; i++)
                    {
                        row["Log2(Fold Change)" + i] = Math.Round(xl.log2FoldChange[i - 1], 4);
                        row["p-value" + i] = Math.Round(xl.pValue[i - 1], 4);
                    }
                }
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
            dtResidues.Columns.Add("α position", typeof(int));
            dtResidues.Columns.Add("β position", typeof(int));
            dtResidues.Columns.Add("Spec count", typeof(int));

            var filteredResidues = FilteredResults.ResidueSearchResults;

            int qtdUniqueClass = Regex.Split(FilteredResults.Params.ClassLabels, " ").Distinct().Count();
            if (qtdUniqueClass > 1)
            {
                for (int i = 1; i <= qtdUniqueClass - 1; i++)
                    dtResidues.Columns.Add("Log2(Fold Change)" + i, typeof(double));
                for (int i = 1; i <= qtdUniqueClass - 1; i++)
                    dtResidues.Columns.Add("p-value" + i, typeof(double));

                filteredResidues = FilteredResults.ResidueSearchResults.OrderByDescending(a => a.log2FoldChange[0]).ThenByDescending(a => a.cSMs.Count).ToList();
            }

            foreach (XLSearchResult xl in filteredResidues)
            {
                var row = dtResidues.NewRow();
                row["Gene A"] = xl.cSMs[0].genes_alpha[0];
                row["Gene B"] = xl.cSMs[0].genes_beta[0];
                row["α position"] = xl.cSMs[0].alpha_pept_xl_pos;
                row["β position"] = xl.cSMs[0].beta_pept_xl_pos;
                row["Spec count"] = xl.cSMs.Count;

                if (qtdUniqueClass > 1)
                {
                    for (int i = 1; i <= qtdUniqueClass - 1; i++)
                    {
                        row["Log2(Fold Change)" + i] = Math.Round(xl.log2FoldChange[i - 1], 4);
                        row["p-value" + i] = Math.Round(xl.pValue[i - 1], 4);
                    }
                }

                dtResidues.Rows.Add(row);
            }

            return dtResidues;
        }

        /// <summary>
        /// Method responsible for creating data table for ppis
        /// </summary>
        /// <returns></returns>
        private DataTable createDataTablePPI()
        {
            DataTable dtPPI = new DataTable();

            dtPPI.Columns.Add("Gene A");
            dtPPI.Columns.Add("Gene B");
            dtPPI.Columns.Add("Protein A");
            dtPPI.Columns.Add("Protein B");
            dtPPI.Columns.Add("PPI score", typeof(double));
            dtPPI.Columns.Add("XL count", typeof(int));
            dtPPI.Columns.Add("Spec count", typeof(int));

            var filteredPPI = FilteredResults.PPIResults;

            int qtdUniqueClass = Regex.Split(FilteredResults.Params.ClassLabels, " ").Distinct().Count();
            if (qtdUniqueClass > 1)
            {
                for (int i = 1; i <= qtdUniqueClass - 1; i++)
                    dtPPI.Columns.Add("Log2(Fold Change)" + i, typeof(double));
                for (int i = 1; i <= qtdUniqueClass - 1; i++)
                    dtPPI.Columns.Add("p-value" + i, typeof(double));

                filteredPPI = FilteredResults.PPIResults.OrderByDescending(a => a.log2FoldChange[0]).ThenByDescending(a => a.specCount).ToList();
            }

            foreach (ProteinProteinInteraction ppi in filteredPPI)
            {
                if (ppi.XLs == null || ppi.XLs.Count < Utils.Utils.MIN_CROSSLINKEDPEPTIDES || ppi.specCount < Utils.Utils.SPEC_COUNT) continue;

                var row = dtPPI.NewRow();
                row["Gene A"] = ppi.gene_a;
                row["Gene B"] = ppi.gene_b;
                row["Protein A"] = ppi.protein_a;
                row["Protein B"] = ppi.protein_b;
                row["PPI score"] = Utils.Utils.RoundUp(ppi.score, 30);
                row["Spec count"] = ppi.specCount;
                row["XL count"] = ppi.XLs.Count;

                if (qtdUniqueClass > 1)
                {
                    if (ppi.pValue[0] == 0 && ppi.log2FoldChange[0] == 0) continue;

                    for (int i = 1; i <= qtdUniqueClass - 1; i++)
                    {
                        row["Log2(Fold Change)" + i] = Math.Round(ppi.log2FoldChange[i - 1], 4);
                        row["p-value" + i] = Math.Round(ppi.pValue[i - 1], 4);
                    }
                }
                dtPPI.Rows.Add(row);
            }

            return dtPPI;
        }

        /// <summary>
        /// Method responsible for cloning MyResults object
        /// </summary>
        private void cloneResults()
        {
            if (IsobaricAnalyzerControl.resultsPackage == null) return;

            #region Cloning MyResults
            FilteredResults = new();
            IsobaricAnalyzerControl.resultsPackage.CSMSearchResults.ForEach((item) =>
            {
                CSMSearchResult csm = item.ShallowCopy();
                FilteredResults.CSMSearchResults.Add(csm);
            });

            IsobaricAnalyzerControl.resultsPackage.XLSearchResults.ForEach((item) =>
            {
                XLSearchResult xlsr = item.ShallowCopy();
                FilteredResults.XLSearchResults.Add(xlsr);
            });

            IsobaricAnalyzerControl.resultsPackage.ResidueSearchResults.ForEach((item) =>
            {
                XLSearchResult residue = item.ShallowCopy();
                FilteredResults.ResidueSearchResults.Add(residue);
            });

            IsobaricAnalyzerControl.resultsPackage.PPIResults.ForEach((item) =>
            {
                ProteinProteinInteraction ppi = item.ShallowCopy();
                FilteredResults.PPIResults.Add(ppi);
            });

            FilteredResults.FileNameIndex = IsobaricAnalyzerControl.resultsPackage.FileNameIndex;
            FilteredResults.Params = IsobaricAnalyzerControl.resultsPackage.Params;
            FilteredResults.Spectra = IsobaricAnalyzerControl.resultsPackage.Spectra;
            #endregion
        }

        /// <summary>
        /// Method responsible for filtering results based on parameters set in the interface
        /// </summary>
        private void applyFilter()
        {
            cloneResults();

            #region log2 fold change & p-value
            if (FilteredResults == null) return;

            int qtdFoldChange = Regex.Split(FilteredResults.Params.ClassLabels, " ").Distinct().Count() - 1;

            if (qtdFoldChange > 0)
            {
                FilteredResults.CSMSearchResults = FilteredResults.CSMSearchResults.Where(a => a.log2FoldChange != null && a.pValue != null && a.log2FoldChange.Any(b => Math.Abs(b) >= Utils.Utils.FOLD_CHANGE_CUTOFF) && a.pValue.Any(b => b <= Utils.Utils.PVALUE_CUTOFF)).ToList();
                FilteredResults.XLSearchResults = FilteredResults.XLSearchResults.Where(a => a.cSMs != null && a.cSMs.Count >= Utils.Utils.SPEC_COUNT && a.log2FoldChange != null && a.pValue != null && a.log2FoldChange.Any(b => Math.Abs(b) >= Utils.Utils.FOLD_CHANGE_CUTOFF) && a.pValue.Any(b => b <= Utils.Utils.PVALUE_CUTOFF)).ToList();
                FilteredResults.ResidueSearchResults = FilteredResults.ResidueSearchResults.Where(a => a.cSMs != null && a.cSMs.Count >= Utils.Utils.SPEC_COUNT && a.log2FoldChange != null && a.pValue != null && a.log2FoldChange.Any(b => Math.Abs(b) >= Utils.Utils.FOLD_CHANGE_CUTOFF) && a.pValue.Any(b => b <= Utils.Utils.PVALUE_CUTOFF)).ToList();
                FilteredResults.PPIResults = FilteredResults.PPIResults.Where(a => a.log2FoldChange != null && a.pValue != null && a.log2FoldChange.Any(b => Math.Abs(b) >= Utils.Utils.FOLD_CHANGE_CUTOFF) && a.pValue.Any(b => b <= Utils.Utils.PVALUE_CUTOFF)).ToList();
                FilteredResults.PPIResults.RemoveAll(a => a.XLs == null || a.XLs.Count < Utils.Utils.MIN_CROSSLINKEDPEPTIDES || a.specCount < Utils.Utils.SPEC_COUNT);
            }
            #endregion

            csm_results_datagrid.ItemsSource = createDataTableCSM().AsDataView();
            xl_results_datagrid.ItemsSource = createDataTableXL().AsDataView();
            residues_results_datagrid.ItemsSource = createDataTableResidue().AsDataView();
            ppi_results_datagrid.ItemsSource = createDataTablePPI().AsDataView();

            var foldChangeLables = information_grid.Children.OfType<Label>().Where(e =>
            e.Name.Contains("foldchangePPI_title_") ||
            e.Name.Contains("foldchangePPI_value_") ||
            e.Name.Contains("foldchangeResidue_title_") ||
            e.Name.Contains("foldchangeResidue_value_") ||
            e.Name.Contains("foldchangeXL_title_") ||
            e.Name.Contains("foldchangeXL_value_") ||
            e.Name.Contains("foldchangeCSM_title_") ||
            e.Name.Contains("foldchangeCSM_value_"));

            while (foldChangeLables.Count() > 0)
            {
                information_grid.Children.Remove(foldChangeLables.ElementAt(0));
            }

            int offsetX = 560;

            for (int i = 0; i < qtdFoldChange; i++)
            {
                var folds = FilteredResults.PPIResults.Select(a => a.log2FoldChange[i]).ToList();

                double median = 0;
                if (folds.Count > 0)
                {
                    if (folds.Count == 1)
                        median = folds[0];
                    else
                    {
                        median = Utils.Utils.Median(folds);
                    }
                }

                Label medianFoldChange_title = new();
                medianFoldChange_title.Name = "foldchangePPI_title_" + (i + 1);
                medianFoldChange_title.Content = qtdFoldChange == 1 ? "Median fold changes:" : "Median fold changes-" + (i + 1) + ":";
                medianFoldChange_title.Margin = new Thickness(offsetX, 40, 0, 0);
                information_grid.Children.Add(medianFoldChange_title);

                Label medianFoldChange = new();
                medianFoldChange.Name = "foldchangePPI_value_" + (i + 1);
                medianFoldChange.Content = Math.Round(median, 4);
                medianFoldChange.Margin = new Thickness(offsetX + 135, 40, 0, 0);
                medianFoldChange.FontWeight = System.Windows.FontWeights.Bold;
                information_grid.Children.Add(medianFoldChange);
                offsetX += 200;
            }

            offsetX = 560;

            for (int i = 0; i < qtdFoldChange; i++)
            {
                var folds = FilteredResults.ResidueSearchResults.Select(a => a.log2FoldChange[i]).ToList();

                double median = 0;
                if (folds.Count > 0)
                {
                    if (folds.Count == 1)
                        median = folds[0];
                    else
                    {
                        median = Utils.Utils.Median(folds);
                    }
                }

                Label medianFoldChange_title = new();
                medianFoldChange_title.Name = "foldchangeResidue_title_" + (i + 1);
                medianFoldChange_title.Content = qtdFoldChange == 1 ? "Median fold changes:" : "Median fold changes-" + (i + 1) + ":";
                medianFoldChange_title.Margin = new Thickness(offsetX, 70, 0, 0);
                information_grid.Children.Add(medianFoldChange_title);

                Label medianFoldChange = new();
                medianFoldChange.Name = "foldchangeResidue_value_" + (i + 1);
                medianFoldChange.Content = Math.Round(median, 4);
                medianFoldChange.Margin = new Thickness(offsetX + 135, 70, 0, 0);
                medianFoldChange.FontWeight = System.Windows.FontWeights.Bold;
                information_grid.Children.Add(medianFoldChange);
                offsetX += 200;
            }

            offsetX = 560;

            for (int i = 0; i < qtdFoldChange; i++)
            {
                var folds = FilteredResults.XLSearchResults.Select(a => a.log2FoldChange[i]).ToList();

                double median = 0;
                if (folds.Count > 0)
                {
                    if (folds.Count == 1)
                        median = folds[0];
                    else
                    {
                        median = Utils.Utils.Median(folds);
                    }
                }

                Label medianFoldChange_title = new();
                medianFoldChange_title.Name = "foldchangeXL_title_" + (i + 1);
                medianFoldChange_title.Content = qtdFoldChange == 1 ? "Median fold changes:" : "Median fold changes-" + (i + 1) + ":";
                medianFoldChange_title.Margin = new Thickness(offsetX, 100, 0, 0);
                information_grid.Children.Add(medianFoldChange_title);

                Label medianFoldChange = new();
                medianFoldChange.Name = "foldchangeXL_value_" + (i + 1);
                medianFoldChange.Content = Math.Round(median, 4);
                medianFoldChange.Margin = new Thickness(offsetX + 135, 100, 0, 0);
                medianFoldChange.FontWeight = System.Windows.FontWeights.Bold;
                information_grid.Children.Add(medianFoldChange);
                offsetX += 200;
            }

            offsetX = 560;

            for (int i = 0; i < qtdFoldChange; i++)
            {
                var folds = FilteredResults.CSMSearchResults.Select(a => a.log2FoldChange[i]).ToList();

                double median = 0;
                if (folds.Count > 0)
                {
                    if (folds.Count == 1)
                        median = folds[0];
                    else
                    {
                        median = Utils.Utils.Median(folds);
                    }
                }

                Label medianFoldChange_title = new();
                medianFoldChange_title.Name = "foldchangeCSM_title_" + (i + 1);
                medianFoldChange_title.Content = qtdFoldChange == 1 ? "Median fold changes:" : "Median fold changes-" + (i + 1) + ":";
                medianFoldChange_title.Margin = new Thickness(offsetX, 130, 0, 0);
                information_grid.Children.Add(medianFoldChange_title);

                Label medianFoldChange = new();
                medianFoldChange.Name = "foldchangeCSM_value_" + (i + 1);
                medianFoldChange.Content = Math.Round(median, 4);
                medianFoldChange.Margin = new Thickness(offsetX + 135, 130, 0, 0);
                medianFoldChange.FontWeight = System.Windows.FontWeights.Bold;
                information_grid.Children.Add(medianFoldChange);
                offsetX += 200;
            }

            plotXLDistribution();
            plotPPIAllDistribution();

            ppi_number.Content = FilteredResults.PPIResults.Count;
            residue_number.Content = FilteredResults.ResidueSearchResults.Count;
            xl_number.Content = FilteredResults.XLSearchResults.Count;
            csm_number.Content = FilteredResults.CSMSearchResults.Count;



        }

        /// <summary>
        /// Method responsible for setting up 'ResultsPackage' object and initialize data grid views
        /// </summary>
        /// <param name="myResults"></param>
        public void Setup(ResultsPackage myResults, string fileName = "")
        {
            IsobaricAnalyzerControl = new();
            IsobaricAnalyzerControl.resultsPackage = myResults;
            IsobaricAnalyzerControl.stdOut_console = false;

            if (String.IsNullOrEmpty(fileName))
                this.Title = "TMT - XL-MS :: Results";
            else
            {
                string[] cols = Regex.Split(fileName, "\\\\");
                this.Title = "TMT - XL-MS :: " + cols[cols.Length - 1];
            }


            applyFilter();
            ppi_results_datagrid.Width = 400;

        }

        /// <summary>
        /// Method responsible for plotting xl volcano plot
        /// </summary>
        private void plotXLDistribution()
        {
            var plotModel1 = new PlotModel() { LegendPosition = LegendPosition.LeftTop };
            plotModel1.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Log2(Fold Change)"
            });

            plotModel1.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "-Log(p-value)"
            });

            var Greenseries = new ScatterSeries { MarkerType = MarkerType.Circle };
            Greenseries.MarkerFill = OxyColors.Green;
            Greenseries.MarkerStroke = OxyColors.Green;
            Greenseries.TrackerFormatString = "\nXL = {XL}\nSpec Count = {SpecCount}\n-Log(p-value) = {2:0.###}\nLog2(Fold change) = {4:0.###}";

            var Redseries = new ScatterSeries { MarkerType = MarkerType.Circle };
            Redseries.MarkerFill = OxyColors.Red;
            Redseries.MarkerStroke = OxyColors.Red;
            Redseries.TrackerFormatString = "\nXL = {XL}\nSpec Count = {SpecCount}\n-Log(p-value) = {2:0.###}\nLog2(Fold change) = {4:0.###}";

            var Grayseries = new ScatterSeries { MarkerType = MarkerType.Circle };
            Grayseries.MarkerFill = OxyColors.Transparent;
            Grayseries.MarkerStroke = OxyColors.Gray;
            Grayseries.TrackerFormatString = "\nXL = {XL}\nSpec Count = {SpecCount}\n-Log(p-value) = {2:0.###}\nLog2(Fold change) = {4:0.###}";

            var Yellowseries = new ScatterSeries { MarkerType = MarkerType.Circle };
            Yellowseries.MarkerFill = OxyColors.Transparent;
            Yellowseries.MarkerStroke = OxyColors.Gold;
            Yellowseries.TrackerFormatString = "\nXL = {XL}\nSpec Count = {SpecCount}\n-Log(p-value) = {2:0.###}\nLog2(Fold change) = {4:0.###}";

            var greenPoints = new List<CustomDataPoint>();
            var redPoints = new List<CustomDataPoint>();
            var grayPoints = new List<CustomDataPoint>();
            var yellowPoints = new List<CustomDataPoint>();

            // base line zero
            var zeroLine = new OxyPlot.Series.LineSeries()
            {
                Color = OxyPlot.OxyColors.Black,
                StrokeThickness = 1,
                LineStyle = LineStyle.Dash,
                MarkerSize = 1,
                MarkerType = OxyPlot.MarkerType.None
            };

            // base line p-value threshold
            var pValueThresholdLine = new OxyPlot.Series.LineSeries()
            {
                Color = OxyPlot.OxyColors.LightGray,
                StrokeThickness = 1.5,
                LineStyle = LineStyle.Dot,
                MarkerSize = 1,
                MarkerType = OxyPlot.MarkerType.None
            };

            // base line fold change upper threshold
            var foldChangeUpperThresholdLine = new OxyPlot.Series.LineSeries()
            {
                Color = OxyPlot.OxyColors.LightGray,
                StrokeThickness = 1.5,
                LineStyle = LineStyle.Dot,
                MarkerSize = 1,
                MarkerType = OxyPlot.MarkerType.None
            };

            // base line fold change lower threshold
            var foldChangeLowerThresholdLine = new OxyPlot.Series.LineSeries()
            {
                Color = OxyPlot.OxyColors.LightGray,
                StrokeThickness = 1.5,
                LineStyle = LineStyle.Dot,
                MarkerSize = 1,
                MarkerType = OxyPlot.MarkerType.None
            };

            double maxPvalue = 0;
            foreach (XLSearchResult xl in IsobaricAnalyzerControl.resultsPackage.XLSearchResults)
            {
                //Skip Quants composed mainly of zeros or quants that have exactly 0.5 as a p-value
                if (xl.cSMs == null || xl.cSMs.Count == 0 || xl.pValue == null || xl.pValue.Count == 0 || xl.pValue[0] == 0.5) { continue; }

                double avgLogFold = xl.log2FoldChange.Average();
                double pValue = Math.Log(xl.pValue.Average(), 10) * (-1);

                //if (avgLogFold < -3) { avgLogFold = -3; }
                //if (avgLogFold > 3) { avgLogFold = 3; }

                int _index = FilteredResults.XLSearchResults.IndexOf(xl);
                if (_index > -1)
                {
                    if (avgLogFold > 0)
                    {
                        if (pValue >= (Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)) &&
                            avgLogFold >= Utils.Utils.FOLD_CHANGE_CUTOFF &&
                            FilteredResults.XLSearchResults[_index].cSMs.Count >= Utils.Utils.SPEC_COUNT) //p-value < 0.05 && fold change > 1 && speccount > 1
                            greenPoints.Add(new CustomDataPoint(pValue, avgLogFold, FilteredResults.XLSearchResults[_index].cSMs.Count, xl.cSMs[0].alpha_peptide + "-" + xl.cSMs[0].beta_peptide, 3));
                        else
                            yellowPoints.Add(new CustomDataPoint(pValue, avgLogFold, FilteredResults.XLSearchResults[_index].cSMs.Count, FilteredResults.XLSearchResults[_index].cSMs[0].alpha_peptide + "-" + xl.cSMs[0].beta_peptide, 3));
                    }
                    else
                    {
                        if (pValue >= (Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)) &&
                            avgLogFold <= -Utils.Utils.FOLD_CHANGE_CUTOFF &&
                            FilteredResults.XLSearchResults[_index].cSMs.Count >= Utils.Utils.SPEC_COUNT) //p-value < 0.05 && fold change > 1 && speccount > 1
                            redPoints.Add(new CustomDataPoint(pValue, avgLogFold, FilteredResults.XLSearchResults[_index].cSMs.Count, xl.cSMs[0].alpha_peptide + "-" + xl.cSMs[0].beta_peptide, 3));
                        else
                            yellowPoints.Add(new CustomDataPoint(pValue, avgLogFold, FilteredResults.XLSearchResults[_index].cSMs.Count, xl.cSMs[0].alpha_peptide + "-" + xl.cSMs[0].beta_peptide, 3));
                    }

                    if (maxPvalue < pValue) maxPvalue = pValue;
                }
                else
                {
                    if (pValue >= (Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)) &&
                            Math.Abs(avgLogFold) >= Utils.Utils.FOLD_CHANGE_CUTOFF)
                    {
                        yellowPoints.Add(new CustomDataPoint(pValue, avgLogFold, xl.cSMs.Count, xl.cSMs[0].alpha_peptide + "-" + xl.cSMs[0].beta_peptide, 3));
                        if (maxPvalue < pValue) maxPvalue = pValue;
                    }
                    else
                        grayPoints.Add(new CustomDataPoint(pValue, avgLogFold, xl.cSMs.Count, xl.cSMs[0].alpha_peptide + "-" + xl.cSMs[0].beta_peptide, 3));
                }
            }

            zeroLine.Points.Add(new OxyPlot.DataPoint(0, 0));
            zeroLine.Points.Add(new OxyPlot.DataPoint(maxPvalue, 0));
            pValueThresholdLine.Points.Add(new OxyPlot.DataPoint((Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)), 3));
            pValueThresholdLine.Points.Add(new OxyPlot.DataPoint((Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)), -3));
            foldChangeUpperThresholdLine.Points.Add(new OxyPlot.DataPoint(0, Utils.Utils.FOLD_CHANGE_CUTOFF));
            foldChangeUpperThresholdLine.Points.Add(new OxyPlot.DataPoint(maxPvalue, Utils.Utils.FOLD_CHANGE_CUTOFF));
            foldChangeLowerThresholdLine.Points.Add(new OxyPlot.DataPoint(0, -Utils.Utils.FOLD_CHANGE_CUTOFF));
            foldChangeLowerThresholdLine.Points.Add(new OxyPlot.DataPoint(maxPvalue, -Utils.Utils.FOLD_CHANGE_CUTOFF));

            grayPoints.RemoveAll(a => a.X > maxPvalue);
            yellowPoints.RemoveAll(a => a.X > maxPvalue);
            Greenseries.ItemsSource = greenPoints;
            Yellowseries.ItemsSource = yellowPoints;
            Redseries.ItemsSource = redPoints;
            Grayseries.ItemsSource = grayPoints;
            plotModel1.Series.Add(Grayseries);
            plotModel1.Series.Add(Greenseries);
            plotModel1.Series.Add(Redseries);
            plotModel1.Series.Add(Yellowseries);
            plotModel1.Series.Add(zeroLine);
            plotModel1.Series.Add(pValueThresholdLine);
            plotModel1.Series.Add(foldChangeUpperThresholdLine);
            plotModel1.Series.Add(foldChangeLowerThresholdLine);

            xl_plot.Model = plotModel1;

            //var pointAnnotationWT = new PointAnnotation()
            //{
            //    X = 0.05,
            //    Y = xl_plot.Model.Axes[0].ActualMinimum - (xl_plot.Model.Axes[0].ActualMinimum * 0.3),
            //    Text = "Condition 2",
            //    Shape = MarkerType.None
            //};
            //var pointAnnotationControl = new PointAnnotation()
            //{
            //    X = 0.05,
            //    Y = xl_plot.Model.Axes[0].ActualMaximum - (xl_plot.Model.Axes[0].ActualMaximum * 0.15),
            //    Text = "Condition 1",
            //    Shape = MarkerType.None
            //};
            //xl_plot.Model.Annotations.Add(pointAnnotationWT);
            //xl_plot.Model.Annotations.Add(pointAnnotationControl);
        }

        /// <summary>
        /// Method responsible for plotting PPI volcano plot
        /// </summary>
        private void plotPPIAllDistribution()
        {
            var plotModel1 = new PlotModel() { LegendPosition = LegendPosition.LeftTop };
            plotModel1.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Log2(Fold Change)"
            });

            plotModel1.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "-Log(p-value)"
            });


            var Greenseries = new ScatterSeries { MarkerType = MarkerType.Circle };
            Greenseries.MarkerFill = OxyColors.Green;
            Greenseries.MarkerStroke = OxyColors.Green;
            Greenseries.TrackerFormatString = "\nPPI = {XL}\n#XL = {XLCount}\nSpec Count = {SpecCount}\n-Log(p-value) = {2:0.###}\nLog2(Fold change) = {4:0.###}";

            var Redseries = new ScatterSeries { MarkerType = MarkerType.Circle };
            Redseries.MarkerFill = OxyColors.Red;
            Redseries.MarkerStroke = OxyColors.Red;
            Redseries.TrackerFormatString = "\nPPI = {XL}\n#XL = {XLCount}\nSpec Count = {SpecCount}\n-Log(p-value) = {2:0.###}\nLog2(Fold change) = {4:0.###}";

            var Grayseries = new ScatterSeries { MarkerType = MarkerType.Circle };
            Grayseries.MarkerFill = OxyColors.Transparent;
            Grayseries.MarkerStroke = OxyColors.LightGray;
            Grayseries.TrackerFormatString = "\nPPI = {XL}\n#XL = {XLCount}\nSpec Count = {SpecCount}\n-Log(p-value) = {2:0.###}\nLog2(Fold change) = {4:0.###}";

            var Yellowseries = new ScatterSeries { MarkerType = MarkerType.Circle };
            Yellowseries.MarkerFill = OxyColors.Transparent;
            Yellowseries.MarkerStroke = OxyColors.Gray;
            Yellowseries.TrackerFormatString = "\nPPI = {XL}\n#XL = {XLCount}\nSpec Count = {SpecCount}\n-Log(p-value) = {2:0.###}\nLog2(Fold change) = {4:0.###}";

            var greenPoints = new List<CustomDataPoint>();
            var redPoints = new List<CustomDataPoint>();
            var grayPoints = new List<CustomDataPoint>();
            var yellowPoints = new List<CustomDataPoint>();

            // base line zero
            var zeroLine = new OxyPlot.Series.LineSeries()
            {
                Color = OxyPlot.OxyColors.Black,
                StrokeThickness = 1,
                LineStyle = LineStyle.Dash,
                MarkerSize = 1,
                MarkerType = OxyPlot.MarkerType.None
            };

            // base line p-value threshold
            var pValueThresholdLine = new OxyPlot.Series.LineSeries()
            {
                Color = OxyPlot.OxyColors.LightGray,
                StrokeThickness = 1.5,
                LineStyle = LineStyle.Dot,
                MarkerSize = 1,
                MarkerType = OxyPlot.MarkerType.None
            };

            // base line fold change upper threshold
            var foldChangeUpperThresholdLine = new OxyPlot.Series.LineSeries()
            {
                Color = OxyPlot.OxyColors.LightGray,
                StrokeThickness = 1.5,
                LineStyle = LineStyle.Dot,
                MarkerSize = 1,
                MarkerType = OxyPlot.MarkerType.None
            };

            // base line fold change lower threshold
            var foldChangeLowerThresholdLine = new OxyPlot.Series.LineSeries()
            {
                Color = OxyPlot.OxyColors.LightGray,
                StrokeThickness = 1.5,
                LineStyle = LineStyle.Dot,
                MarkerSize = 1,
                MarkerType = OxyPlot.MarkerType.None
            };

            double maxPvalue = 0;
            foreach (ProteinProteinInteraction ppi in IsobaricAnalyzerControl.resultsPackage.PPIResults)
            {
                //Skip Quants composed mainly of zeros or quants that have exactly 0.5 as a p-value
                if (ppi.pValue == null || ppi.pValue.Count == 0 || ppi.pValue[0] == 0.5 || ppi.XLs == null || ppi.XLs.Count == 0) { continue; }

                double avgLogFold = ppi.log2FoldChange[0];
                double pValue = Math.Log(ppi.pValue[0], 10) * (-1);

                //if (avgLogFold < -3) { avgLogFold = -3; }
                //if (avgLogFold > 3) { avgLogFold = 3; }


                int _index = FilteredResults.PPIResults.IndexOf(ppi);
                if (_index > -1)
                {
                    if (avgLogFold > 0)
                    {
                        if (pValue >= (Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)) && avgLogFold >= Utils.Utils.FOLD_CHANGE_CUTOFF) //p-value < 0.05 && fold change > 1
                            greenPoints.Add(new CustomDataPoint(pValue, avgLogFold, FilteredResults.PPIResults[_index].specCount, FilteredResults.PPIResults[_index].XLs.Count, ppi.gene_a + "-" + ppi.gene_b, 3));
                        else
                            yellowPoints.Add(new CustomDataPoint(pValue, avgLogFold, FilteredResults.PPIResults[_index].specCount, FilteredResults.PPIResults[_index].XLs.Count, ppi.gene_a + "-" + ppi.gene_b, 3));
                    }
                    else
                    {
                        if (pValue >= (Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)) && avgLogFold <= -Utils.Utils.FOLD_CHANGE_CUTOFF) //p-value < 0.05 && fold change < -1
                            redPoints.Add(new CustomDataPoint(pValue, avgLogFold, FilteredResults.PPIResults[_index].specCount, FilteredResults.PPIResults[_index].XLs.Count, ppi.gene_a + "-" + ppi.gene_b, 3));
                        else
                            yellowPoints.Add(new CustomDataPoint(pValue, avgLogFold, FilteredResults.PPIResults[_index].specCount, FilteredResults.PPIResults[_index].XLs.Count, ppi.gene_a + "-" + ppi.gene_b, 3));
                    }

                    if (maxPvalue < pValue) maxPvalue = pValue;
                }
                else
                {
                    yellowPoints.Add(new CustomDataPoint(pValue, avgLogFold, ppi.specCount, ppi.XLs != null ? ppi.XLs.Count : 0, ppi.gene_a + "-" + ppi.gene_b, 3));
                }
            }

            zeroLine.Points.Add(new OxyPlot.DataPoint(0, 0));
            zeroLine.Points.Add(new OxyPlot.DataPoint(maxPvalue, 0));
            pValueThresholdLine.Points.Add(new OxyPlot.DataPoint((Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)), 3));
            pValueThresholdLine.Points.Add(new OxyPlot.DataPoint((Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)), -3));
            foldChangeUpperThresholdLine.Points.Add(new OxyPlot.DataPoint(0, Utils.Utils.FOLD_CHANGE_CUTOFF));
            foldChangeUpperThresholdLine.Points.Add(new OxyPlot.DataPoint(maxPvalue, Utils.Utils.FOLD_CHANGE_CUTOFF));
            foldChangeLowerThresholdLine.Points.Add(new OxyPlot.DataPoint(0, -Utils.Utils.FOLD_CHANGE_CUTOFF));
            foldChangeLowerThresholdLine.Points.Add(new OxyPlot.DataPoint(maxPvalue, -Utils.Utils.FOLD_CHANGE_CUTOFF));

            //grayPoints.RemoveAll(a => a.X > maxPvalue);
            Greenseries.ItemsSource = greenPoints;
            Yellowseries.ItemsSource = yellowPoints;
            Redseries.ItemsSource = redPoints;
            Grayseries.ItemsSource = grayPoints;
            plotModel1.Series.Add(Grayseries);
            plotModel1.Series.Add(Greenseries);
            plotModel1.Series.Add(Redseries);
            plotModel1.Series.Add(Yellowseries);
            plotModel1.Series.Add(zeroLine);
            plotModel1.Series.Add(pValueThresholdLine);
            plotModel1.Series.Add(foldChangeUpperThresholdLine);
            plotModel1.Series.Add(foldChangeLowerThresholdLine);

            ppiAll_plot.Model = plotModel1;
        }

        /// <summary>
        /// Method responsible for plotting PPI volcano plot in the PPI tab
        /// </summary>
        /// <param name="xlSearchResults"></param>
        private void plotPPIDistribution(List<XLSearchResult> xlSearchResults)
        {
            if (xlSearchResults == null) return;

            var plotModel1 = new PlotModel() { LegendPosition = LegendPosition.LeftTop };
            plotModel1.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Log2(Fold Change)"
            });

            plotModel1.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "-Log(p-value)"
            });


            var Greenseries = new ScatterSeries { MarkerType = MarkerType.Circle };
            Greenseries.MarkerFill = OxyColors.Green;
            Greenseries.MarkerStroke = OxyColors.Green;
            Greenseries.TrackerFormatString = "\nXL = {XL}\nQuant = {SpecCount}\n-Log(p-value) = {2:0.###}\nLog2(Fold change) = {4:0.###}";

            var Redseries = new ScatterSeries { MarkerType = MarkerType.Circle };
            Redseries.MarkerFill = OxyColors.Red;
            Redseries.MarkerStroke = OxyColors.Red;
            Redseries.TrackerFormatString = "\nXL = {XL}\nQuant = {SpecCount}\n-Log(p-value) = {2:0.###}\nLog2(Fold change) = {4:0.###}";

            var Grayseries = new ScatterSeries { MarkerType = MarkerType.Circle };
            Grayseries.MarkerFill = OxyColors.Transparent;
            Grayseries.MarkerStroke = OxyColors.LightGray;
            Grayseries.TrackerFormatString = "\nXL = {XL}\nQuant = {SpecCount}\n-Log(p-value) = {2:0.###}\nLog2(Fold change) = {4:0.###}";

            var Yellowseries = new ScatterSeries { MarkerType = MarkerType.Circle };
            Yellowseries.MarkerFill = OxyColors.Transparent;
            Yellowseries.MarkerStroke = OxyColors.Gray;
            Yellowseries.TrackerFormatString = "\nXL = {XL}\nQuant = {SpecCount}\n-Log(p-value) = {2:0.###}\nLog2(Fold change) = {4:0.###}";

            var greenPoints = new List<CustomDataPoint>();
            var redPoints = new List<CustomDataPoint>();
            var grayPoints = new List<CustomDataPoint>();
            var yellowPoints = new List<CustomDataPoint>();

            // base line zero
            var zeroLine = new OxyPlot.Series.LineSeries()
            {
                Color = OxyPlot.OxyColors.Black,
                StrokeThickness = 1,
                LineStyle = LineStyle.Dash,
                MarkerSize = 1,
                MarkerType = OxyPlot.MarkerType.None
            };

            // base line p-value threshold
            var pValueThresholdLine = new OxyPlot.Series.LineSeries()
            {
                Color = OxyPlot.OxyColors.LightGray,
                StrokeThickness = 1.5,
                LineStyle = LineStyle.Dot,
                MarkerSize = 1,
                MarkerType = OxyPlot.MarkerType.None
            };

            // base line fold change upper threshold
            var foldChangeUpperThresholdLine = new OxyPlot.Series.LineSeries()
            {
                Color = OxyPlot.OxyColors.LightGray,
                StrokeThickness = 1.5,
                LineStyle = LineStyle.Dot,
                MarkerSize = 1,
                MarkerType = OxyPlot.MarkerType.None
            };

            // base line fold change lower threshold
            var foldChangeLowerThresholdLine = new OxyPlot.Series.LineSeries()
            {
                Color = OxyPlot.OxyColors.LightGray,
                StrokeThickness = 1.5,
                LineStyle = LineStyle.Dot,
                MarkerSize = 1,
                MarkerType = OxyPlot.MarkerType.None
            };

            double maxPvalue = 0;
            foreach (XLSearchResult xl in xlSearchResults)
            {
                //Skip Quants composed mainly of zeros or quants that have exactly 0.5 as a p-value
                if (xl.pValue == null || xl.pValue.Count == 0 || xl.pValue[0] == 0.5) { continue; }

                double avgLogFold = xl.log2FoldChange[0];
                double pValue = Math.Log(xl.pValue[0], 10) * (-1);

                //if (avgLogFold < -3) { avgLogFold = -3; }
                //if (avgLogFold > 3) { avgLogFold = 3; }

                int _index = FilteredResults.XLSearchResults.IndexOf(xl);
                if (_index > -1)
                {
                    if (avgLogFold > 0)
                    {
                        if (pValue >= (Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)) &&
                            avgLogFold >= Utils.Utils.FOLD_CHANGE_CUTOFF &&
                            FilteredResults.XLSearchResults[_index].cSMs.Count >= Utils.Utils.SPEC_COUNT) //p-value < 0.05 && fold change > 1 && speccount > 1
                            greenPoints.Add(new CustomDataPoint(pValue, avgLogFold, FilteredResults.XLSearchResults[_index].cSMs.Count, xl.cSMs[0].alpha_peptide + "-" + xl.cSMs[0].beta_peptide, 3));
                        else
                            yellowPoints.Add(new CustomDataPoint(pValue, avgLogFold, FilteredResults.XLSearchResults[_index].cSMs.Count, FilteredResults.XLSearchResults[_index].cSMs[0].alpha_peptide + "-" + xl.cSMs[0].beta_peptide, 3));
                    }
                    else
                    {
                        if (pValue >= (Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)) &&
                            avgLogFold <= -Utils.Utils.FOLD_CHANGE_CUTOFF &&
                            FilteredResults.XLSearchResults[_index].cSMs.Count >= Utils.Utils.SPEC_COUNT) //p-value < 0.05 && fold change > 1 && speccount > 1
                            redPoints.Add(new CustomDataPoint(pValue, avgLogFold, FilteredResults.XLSearchResults[_index].cSMs.Count, xl.cSMs[0].alpha_peptide + "-" + xl.cSMs[0].beta_peptide, 3));
                        else
                            yellowPoints.Add(new CustomDataPoint(pValue, avgLogFold, FilteredResults.XLSearchResults[_index].cSMs.Count, xl.cSMs[0].alpha_peptide + "-" + xl.cSMs[0].beta_peptide, 3));
                    }

                    if (maxPvalue < pValue) maxPvalue = pValue;
                }
                else
                {
                    if (pValue >= (Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)) &&
                            Math.Abs(avgLogFold) >= Utils.Utils.FOLD_CHANGE_CUTOFF)
                    {
                        yellowPoints.Add(new CustomDataPoint(pValue, avgLogFold, xl.cSMs.Count, xl.cSMs[0].alpha_peptide + "-" + xl.cSMs[0].beta_peptide, 3));
                        if (maxPvalue < pValue) maxPvalue = pValue;
                    }
                    else
                        grayPoints.Add(new CustomDataPoint(pValue, avgLogFold, xl.cSMs.Count, xl.cSMs[0].alpha_peptide + "-" + xl.cSMs[0].beta_peptide, 3));
                }
            }

            zeroLine.Points.Add(new OxyPlot.DataPoint(0, 0));
            zeroLine.Points.Add(new OxyPlot.DataPoint(maxPvalue, 0));
            pValueThresholdLine.Points.Add(new OxyPlot.DataPoint((Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)), 3));
            pValueThresholdLine.Points.Add(new OxyPlot.DataPoint((Math.Log(Utils.Utils.PVALUE_CUTOFF, 10) * (-1)), -3));
            foldChangeUpperThresholdLine.Points.Add(new OxyPlot.DataPoint(0, Utils.Utils.FOLD_CHANGE_CUTOFF));
            foldChangeUpperThresholdLine.Points.Add(new OxyPlot.DataPoint(maxPvalue, Utils.Utils.FOLD_CHANGE_CUTOFF));
            foldChangeLowerThresholdLine.Points.Add(new OxyPlot.DataPoint(0, -Utils.Utils.FOLD_CHANGE_CUTOFF));
            foldChangeLowerThresholdLine.Points.Add(new OxyPlot.DataPoint(maxPvalue, -Utils.Utils.FOLD_CHANGE_CUTOFF));

            //grayPoints.RemoveAll(a => a.X > maxPvalue);
            Greenseries.ItemsSource = greenPoints;
            Yellowseries.ItemsSource = yellowPoints;
            Redseries.ItemsSource = redPoints;
            Grayseries.ItemsSource = grayPoints;
            plotModel1.Series.Add(Grayseries);
            plotModel1.Series.Add(Greenseries);
            plotModel1.Series.Add(Redseries);
            plotModel1.Series.Add(Yellowseries);
            plotModel1.Series.Add(zeroLine);
            plotModel1.Series.Add(pValueThresholdLine);
            plotModel1.Series.Add(foldChangeUpperThresholdLine);
            plotModel1.Series.Add(foldChangeLowerThresholdLine);

            ppi_plot.Model = plotModel1;
            ppi_results_datagrid.Width = tabControl.ActualWidth / 2 - 40;
            ppi_plot.Width = tabControl.ActualWidth - ppi_results_datagrid.ActualWidth - 20;
        }

        /// <summary>
        /// Method responsible for getting datagrid selected value
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        private string GetSelectedValue(DataGrid grid, int columnIndex = 0)
        {
            if (grid.SelectedCells.Count == 0) return string.Empty;

            DataGridCellInfo cellInfo = grid.SelectedCells[columnIndex];
            if (cellInfo.Column == null) return "0";

            DataGridBoundColumn column = cellInfo.Column as DataGridBoundColumn;
            if (column == null) return null;

            FrameworkElement element = new FrameworkElement() { DataContext = cellInfo.Item };
            BindingOperations.SetBinding(element, TagProperty, column.Binding);

            return element.Tag.ToString();
        }

        private void csm_results_datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string getValue = GetSelectedValue(csm_results_datagrid);
            if (String.IsNullOrEmpty(getValue)) return;

            int scanNumber = Convert.ToInt32(getValue);

            CSMSearchResult csm = FilteredResults.CSMSearchResults.Where(a => a.scanNumber == scanNumber).FirstOrDefault();

            MSUltraLight ms = FilteredResults.Spectra.Where(a => a.ScanNumber == csm.scanNumber && a.FileNameIndex == csm.fileIndex).FirstOrDefault();

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

        private void results_datagrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void xl_results_datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string gene_a = GetSelectedValue(xl_results_datagrid);
            if (String.IsNullOrEmpty(gene_a)) return;

            string gene_b = GetSelectedValue(xl_results_datagrid, 1);
            string alpha_pept = GetSelectedValue(xl_results_datagrid, 2);
            string beta_pept = GetSelectedValue(xl_results_datagrid, 3);
            int alpha_pos = Convert.ToInt32(GetSelectedValue(xl_results_datagrid, 4));
            int beta_pos = Convert.ToInt32(GetSelectedValue(xl_results_datagrid, 5));

            XLSearchResult current_xlSeachResult = FilteredResults.XLSearchResults.Where(a => a.cSMs.Any(b => b.genes_alpha.Contains(gene_a) &&
            b.genes_beta.Contains(gene_b) &&
            b.alpha_peptide.Equals(alpha_pept) &&
            b.beta_peptide.Equals(beta_pept) &&
            b.alpha_pept_xl_pos == alpha_pos &&
            b.beta_pept_xl_pos == beta_pos)).FirstOrDefault();

            if (current_xlSeachResult == null) return;

            ChannelComparison channelComparison = new ChannelComparison();
            channelComparison.Setup(current_xlSeachResult, FilteredResults.FileNameIndex, FilteredResults.Params);
            channelComparison.ShowDialog();
        }

        private void XLPlotMenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            string getValue = GetSelectedValue(csm_results_datagrid);
            if (String.IsNullOrEmpty(getValue)) return;

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Volcano plot";
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG files (.png)|*.png";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    xl_plot.SaveBitmap(dlg.FileName);
                    System.Windows.Forms.MessageBox.Show("The plot has been saved successfully!", "Information", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("ERROR: " + exc.Message);
                    System.Windows.Forms.MessageBox.Show("Failed to save!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
        }

        private void PPIPlotMenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            string getValue = GetSelectedValue(csm_results_datagrid);
            if (String.IsNullOrEmpty(getValue)) return;

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Volcano plot";
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG files (.png)|*.png";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    ppi_plot.SaveBitmap(dlg.FileName);
                    System.Windows.Forms.MessageBox.Show("The plot has been saved successfully!", "Information", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("ERROR: " + exc.Message);
                    System.Windows.Forms.MessageBox.Show("Failed to save!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
        }

        private void ppi_results_datagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string gene_a = GetSelectedValue(ppi_results_datagrid);
            if (String.IsNullOrEmpty(gene_a)) return;

            string gene_b = GetSelectedValue(ppi_results_datagrid, 1);

            List<XLSearchResult> xlSeachResult = IsobaricAnalyzerControl.resultsPackage.XLSearchResults.Where(a => a.cSMs != null && a.cSMs.Any(b => b.genes_alpha.Contains(gene_a) &&
            b.genes_beta.Contains(gene_b))).ToList();

            if (xlSeachResult == null || xlSeachResult.Count == 0) return;

            plotPPIDistribution(xlSeachResult);
        }

        private void PPIAllPlotMenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            string getValue = GetSelectedValue(csm_results_datagrid);
            if (String.IsNullOrEmpty(getValue)) return;

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Volcano plot";
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG files (.png)|*.png";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    ppiAll_plot.SaveBitmap(dlg.FileName);
                    System.Windows.Forms.MessageBox.Show("The plot has been saved successfully!", "Information", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("ERROR: " + exc.Message);
                    System.Windows.Forms.MessageBox.Show("Failed to save!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
        }

        private void CommandBindingOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private async void CommandBindingOpen_Executed(object sender, ExecutedRoutedEventArgs e)
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
                    if (IsobaricAnalyzerControl == null)
                        IsobaricAnalyzerControl = new();

                    #region Disabling some fields
                    filter_params_groupbox.IsEnabled = false;
                    results_grid.IsEnabled = false;
                    #endregion

                    await Task.Run(
                                () =>
                                {
                                    Console.WriteLine();
                                    IsobaricAnalyzerControl.LoadResults(dlg.FileName);
                                });

                    this.Setup(IsobaricAnalyzerControl.resultsPackage, dlg.FileName);

                    #region Enabling some fields
                    filter_params_groupbox.IsEnabled = true;
                    results_grid.IsEnabled = true;
                    #endregion

                    System.Windows.Forms.MessageBox.Show("The results have been loaded successfully!", "Information", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("ERROR: " + exc.Message);
                    System.Windows.Forms.MessageBox.Show("Failed to load!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
        }

        private void CommandBindingSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private async void CommandBindingSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (FilteredResults == null || FilteredResults.CSMSearchResults == null || FilteredResults.CSMSearchResults.Count == 0)
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
                try
                {
                    await Task.Run(
                                () =>
                                {
                                    IsobaricAnalyzerControl.SaveReults(dlg.FileName);
                                });

                    System.Windows.Forms.MessageBox.Show("The results have been saved successfully!", "Information", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("ERROR: " + exc.Message);
                    System.Windows.Forms.MessageBox.Show("Failed to save!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ppi_results_datagrid.Width = tabControl.ActualWidth / 2 - 40;
            ppi_plot.Width = tabControl.ActualWidth / 2 - 20;
        }

        private void filter_btn_Click(object sender, RoutedEventArgs e)
        {
            Utils.Utils.SPEC_COUNT = (int)IntegerUpDownSpecCount.Value;
            Utils.Utils.MIN_CROSSLINKEDPEPTIDES = (int)IntegerUpDownNoPeptides.Value;
            Utils.Utils.FOLD_CHANGE_CUTOFF = (double)IntegerUpDownFoldChangeCutoff.Value;
            Utils.Utils.PVALUE_CUTOFF = (double)IntegerUpDownPvalueCutoff.Value;

            applyFilter();
        }
    }

    public class CustomDataPoint : IScatterPointProvider
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int SpecCount { get; set; }
        public int XLCount { get; set; }
        public string XL { get; set; }
        public int Size { get; set; }
        public ScatterPoint GetScatterPoint() => new ScatterPoint(X, Y, Size, Size);

        public CustomDataPoint(double x, double y, int specCount, string xl, int size)
        {
            X = x;
            Y = y;
            SpecCount = specCount;
            XL = xl;
            Size = size;
        }

        public CustomDataPoint(double x, double y, int specCount, int xlCount, string xl, int size)
        {
            X = x;
            Y = y;
            SpecCount = specCount;
            XLCount = xlCount;
            XL = xl;
            Size = size;
        }
    }
}
