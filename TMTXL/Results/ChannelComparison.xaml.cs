using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    /// Interaction logic for ChannelComparison.xaml
    /// </summary>
    public partial class ChannelComparison : Window
    {
        private XLSearchResult XLSearchResults;
        private List<string> FileNameIndex;
        private ProgramParams Params;

        public ChannelComparison()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Method responsible for crete data table for CSMs
        /// </summary>
        /// <returns>data table</returns>
        private DataTable createDataTableChannel()
        {
            DataTable dtCSM = new DataTable();

            dtCSM.Columns.Add("Scan Number", typeof(int));
            dtCSM.Columns.Add("File Name");
            dtCSM.Columns.Add("Gene A");
            dtCSM.Columns.Add("Gene B");
            dtCSM.Columns.Add("α position", typeof(int));
            dtCSM.Columns.Add("β position", typeof(int));
            switch (Params.ChemicalLabel)
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
            int qtdUniqueClass = Regex.Split(Params.ClassLabels, " ").Distinct().Count();
            for (int i = 1; i <= qtdUniqueClass; i++)
                dtCSM.Columns.Add("Avg " + i + " not Null", typeof(double));
            for (int i = 1; i <= qtdUniqueClass - 1; i++)
                dtCSM.Columns.Add("Log2(Fold Change)" + i, typeof(double));
            for (int i = 1; i <= qtdUniqueClass - 1; i++)
                dtCSM.Columns.Add("p-value" + i, typeof(double));

            foreach (CSMSearchResult csm in XLSearchResults.cSMs)
            {
                var row = dtCSM.NewRow();
                row["Scan Number"] = csm.scanNumber;
                row["File Name"] = FileNameIndex[csm.fileIndex] + ".raw";
                row["Gene A"] = String.Join(" ,", csm.genes_alpha);
                row["Gene B"] = String.Join(" ,", csm.genes_beta);
                row["α position"] = csm.alpha_pos_xl;
                row["β position"] = csm.beta_pos_xl;
                switch (Params.ChemicalLabel)
                {
                    case "iTRAQ 4":
                        row["114"] = csm.quantitation[0] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[0], 4) : 0;
                        row["115"] = csm.quantitation[1] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[1], 4) : 0;
                        row["116"] = csm.quantitation[2] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[2], 4) : 0;
                        row["117"] = csm.quantitation[3] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[3], 4) : 0;
                        break;
                    case "TMT 6":
                        row["126"] = csm.quantitation[0] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[0], 4) : 0;
                        row["127"] = csm.quantitation[1] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[1], 4) : 0;
                        row["128"] = csm.quantitation[2] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[2], 4) : 0;
                        row["129"] = csm.quantitation[3] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[3], 4) : 0;
                        row["130"] = csm.quantitation[4] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[4], 4) : 0;
                        row["131"] = csm.quantitation[5] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[5], 4) : 0;
                        break;
                    case "TMT 10":
                        row["126"] = csm.quantitation[0] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[0], 4) : 0;
                        row["127N"] = csm.quantitation[1] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[1], 4) : 0;
                        row["127C"] = csm.quantitation[2] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[2], 4) : 0;
                        row["128N"] = csm.quantitation[3] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[3], 4) : 0;
                        row["128C"] = csm.quantitation[4] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[4], 4) : 0;
                        row["129N"] = csm.quantitation[5] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[5], 4) : 0;
                        row["129C"] = csm.quantitation[6] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[6], 4) : 0;
                        row["130N"] = csm.quantitation[7] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[7], 4) : 0;
                        row["130C"] = csm.quantitation[8] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[8], 4) : 0;
                        row["131"] = csm.quantitation[9] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[9], 4) : 0;
                        break;
                    case "TMT 16":
                        row["126"] = csm.quantitation[0] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[0], 4) : 0;
                        row["127N"] = csm.quantitation[1] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[1], 4) : 0;
                        row["127C"] = csm.quantitation[2] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[2], 4) : 0;
                        row["128N"] = csm.quantitation[3] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[3], 4) : 0;
                        row["128C"] = csm.quantitation[4] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[4], 4) : 0;
                        row["129N"] = csm.quantitation[5] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[5], 4) : 0;
                        row["129C"] = csm.quantitation[6] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[6], 4) : 0;
                        row["130N"] = csm.quantitation[7] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[7], 4) : 0;
                        row["130C"] = csm.quantitation[8] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[8], 4) : 0;
                        row["131N"] = csm.quantitation[9] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[9], 4) : 0;
                        row["131C"] = csm.quantitation[10] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[10], 4) : 0;
                        row["132N"] = csm.quantitation[11] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[11], 4) : 0;
                        row["132C"] = csm.quantitation[12] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[12], 4) : 0;
                        row["133N"] = csm.quantitation[13] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[13], 4) : 0;
                        row["133C"] = csm.quantitation[14] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[14], 4) : 0;
                        row["134"] = csm.quantitation[15] > 0 ? Utils.Utils.RoundUp(csm.quantitation[Params.ControlChannel] / csm.quantitation[15], 4) : 0;
                        break;
                }

                for (int i = 1; i <= qtdUniqueClass; i++)
                    row["Avg " + i + " not Null"] = Utils.Utils.RoundUp(csm.avg_notNull[i - 1]);

                for (int i = 1; i <= qtdUniqueClass - 1; i++)
                {
                    row["Log2(Fold Change)" + i] = Math.Round(csm.log2FoldChange[i - 1], 4);
                    row["p-value" + i] = Math.Round(csm.pValue[i - 1], 4);
                }
                dtCSM.Rows.Add(row);
            }

            return dtCSM;
        }

        public void Setup(XLSearchResult xLSearchResults, List<string> fileNameIndex, ProgramParams _params)
        {
            this.XLSearchResults = xLSearchResults;
            this.FileNameIndex = fileNameIndex;
            this.Params = _params;

            channel_datagrid.ItemsSource = createDataTableChannel().AsDataView();
            cross_linked_peptides_title.Text = XLSearchResults.cSMs[0].alpha_peptide + "(" + XLSearchResults.cSMs[0].alpha_pos_xl + ") - (" + XLSearchResults.cSMs[0].beta_pos_xl + ")" + XLSearchResults.cSMs[0].beta_peptide;

            plotChannels();
        }

        private void plotChannels()
        {
            PlotModel MyModel = new PlotModel();
            MyModel.Title = XLSearchResults.cSMs[0].alpha_peptide + "(" + XLSearchResults.cSMs[0].alpha_pos_xl + ") - (" + XLSearchResults.cSMs[0].beta_pos_xl + ")" + XLSearchResults.cSMs[0].beta_peptide;

            MyModel.LegendOrientation = LegendOrientation.Horizontal;
            MyModel.LegendPlacement = LegendPlacement.Outside;
            MyModel.LegendPosition = LegendPosition.BottomCenter;

            var categoryAxis = new CategoryAxis();
            var valueAxis = new LinearAxis();
            valueAxis.Title = "Intensity";
            valueAxis.Position = AxisPosition.Left;
            MyModel.Axes.Add(valueAxis);

            categoryAxis.Labels.Add(new DataColumn("126", typeof(double)).ToString());
            categoryAxis.Labels.Add(new DataColumn("127N", typeof(double)).ToString());
            categoryAxis.Labels.Add(new DataColumn("127C", typeof(double)).ToString());
            categoryAxis.Labels.Add(new DataColumn("128N", typeof(double)).ToString());
            categoryAxis.Labels.Add(new DataColumn("128C", typeof(double)).ToString());
            categoryAxis.Labels.Add(new DataColumn("129N", typeof(double)).ToString());
            categoryAxis.Labels.Add(new DataColumn("129C", typeof(double)).ToString());
            categoryAxis.Labels.Add(new DataColumn("130N", typeof(double)).ToString());
            categoryAxis.Labels.Add(new DataColumn("130C", typeof(double)).ToString());
            categoryAxis.Labels.Add(new DataColumn("131", typeof(double)).ToString());

            MyModel.Axes.Add(categoryAxis);

            foreach (CSMSearchResult csm in XLSearchResults.cSMs)
            {

                var columnSeries = new ColumnSeries();
                columnSeries.Title = "Scan Number: " + csm.scanNumber;

                for (int i = 0; i < csm.quantitation.Count; i++)
                {
                    columnSeries.Items.Add(new ColumnItem(csm.quantitation[i], i));
                }

                MyModel.Series.Add(columnSeries);
            }

            MyPlot.Model = MyModel;

        }

        private void channel_datagrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = XLSearchResults.cSMs[0].alpha_peptide + "(" + XLSearchResults.cSMs[0].alpha_pos_xl + ")-(" + XLSearchResults.cSMs[0].beta_pos_xl + ")" + XLSearchResults.cSMs[0].beta_peptide;
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG files (.png)|*.png";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    MyPlot.SaveBitmap(dlg.FileName);
                    System.Windows.Forms.MessageBox.Show("The plot has been saved successfully!", "Information", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("ERROR: " + exc.Message);
                    System.Windows.Forms.MessageBox.Show("Failed to save!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
        }
    }
}
