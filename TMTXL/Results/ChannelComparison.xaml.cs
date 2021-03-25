using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    /// Interaction logic for ChannelComparison.xaml
    /// </summary>
    public partial class ChannelComparison : Window
    {
        private XLSearchResult XLSearchResults;
        private List<string> FileNameIndex;

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

            foreach (CSMSearchResult csm in XLSearchResults.cSMs)
            {
                var row = dtCSM.NewRow();
                row["Scan Number"] = csm.scanNumber;
                row["File Name"] = FileNameIndex[csm.fileIndex] + ".raw";
                row["Gene A"] = String.Join(" ,", csm.genes_alpha);
                row["Gene B"] = String.Join(" ,", csm.genes_beta);
                row["α position"] = csm.alpha_pos_xl;
                row["β position"] = csm.beta_pos_xl;
                if (true)//TMT
                {
                    row["126"] = Utils.Utils.RoundUp(csm.quantitation[0] / csm.quantitation[0], 4);
                    row["127N"] = Utils.Utils.RoundUp(csm.quantitation[0] / csm.quantitation[1], 4);
                    row["127C"] = Utils.Utils.RoundUp(csm.quantitation[0] / csm.quantitation[2], 4);
                    row["128N"] = Utils.Utils.RoundUp(csm.quantitation[0] / csm.quantitation[3], 4);
                    row["128C"] = Utils.Utils.RoundUp(csm.quantitation[0] / csm.quantitation[4], 4);
                    row["129N"] = Utils.Utils.RoundUp(csm.quantitation[0] / csm.quantitation[5], 4);
                    row["129C"] = Utils.Utils.RoundUp(csm.quantitation[0] / csm.quantitation[6], 4);
                    row["130N"] = Utils.Utils.RoundUp(csm.quantitation[0] / csm.quantitation[7], 4);
                    row["130C"] = Utils.Utils.RoundUp(csm.quantitation[0] / csm.quantitation[8], 4);
                    row["131"] = Utils.Utils.RoundUp(csm.quantitation[0] / csm.quantitation[9], 4);

                    //row["126"] = Utils.Utils.RoundUp(csm.quantitation[0]);
                    //row["127N"] = Utils.Utils.RoundUp(csm.quantitation[1]);
                    //row["127C"] = Utils.Utils.RoundUp(csm.quantitation[2]);
                    //row["128N"] = Utils.Utils.RoundUp(csm.quantitation[3]);
                    //row["128C"] = Utils.Utils.RoundUp(csm.quantitation[4]);
                    //row["129N"] = Utils.Utils.RoundUp(csm.quantitation[5]);
                    //row["129C"] = Utils.Utils.RoundUp(csm.quantitation[6]);
                    //row["130N"] = Utils.Utils.RoundUp(csm.quantitation[7]);
                    //row["130C"] = Utils.Utils.RoundUp(csm.quantitation[8]);
                    //row["131"] = Utils.Utils.RoundUp(csm.quantitation[9]);
                }
                row["Avg 1 not Null"] = Utils.Utils.RoundUp(csm.avg_notNull_1);
                row["Avg 2 not Null"] = Utils.Utils.RoundUp(csm.avg_notNull_2);
                row["Log2(Fold Change)"] = Math.Round(csm.log2FoldChange, 4);
                row["p-value"] = Math.Round(csm.pValue, 4);
                dtCSM.Rows.Add(row);
            }

            return dtCSM;
        }

        public void Setup(XLSearchResult xLSearchResults, List<string> fileNameIndex)
        {
            this.XLSearchResults = xLSearchResults;
            this.FileNameIndex = fileNameIndex;

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
