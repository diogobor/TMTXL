﻿using IsobaricAnalyzer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TMTXL.Control;
using TMTXL.Results;
using TMTXL.Utils;

namespace TMTXL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TextWriter _writer = null; // That's our custom TextWriter class
        private Thread mainThread { get; set; }
        private Program mainProgramGUI { get; set; }

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            ActiveListBoxControl();

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);

            this.DataContext = new OpenResultsBrowserCommandContext();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (mainProgramGUI != null && mainProgramGUI.FinishProcessing)
            {
                mainThread = null;
                dispatcherTimer.Stop();

                if (!mainProgramGUI.ErrorProcessing)
                {
                    System.Windows.MessageBox.Show(
                            "Processing completed succesfully!\nTime: " + mainProgramGUI.FinalTime,
                            "Information",
                            (MessageBoxButton)MessageBoxButtons.OK,
                            (MessageBoxImage)MessageBoxIcon.Information);

                    #region Enabling some fields
                    raw_files_dir.IsEnabled = true;
                    results_dir.IsEnabled = true;
                    raw_files_btn.IsEnabled = true;
                    results_btn.IsEnabled = true;
                    dataGridPurityCorrections.IsEnabled = true;
                    comboBoxPurityDefaults.IsEnabled = true;
                    TabItemAdvancedParams.IsEnabled = true;
                    #endregion

                    run_btn_text.Text = "Run";
                    //change button icon
                    run_btn_img.Source = ToBitmapSource(TMTXL.Properties.Resources.goBtn);

                    if (mainThread != null)//When xlThread is null in this point is because the user stops the process, but before clicking on Yes button, the search is finished.
                    {
                        mainThread.Join();
                        mainThread.Interrupt();
                        mainThread = null;
                    }

                    ResultsWin resultsWin = new ResultsWin();
                    resultsWin.Setup(mainProgramGUI.resultsPackage);
                    resultsWin.ShowDialog();
                }
            }
        }

        private void ActiveListBoxControl()
        {
            // Instantiate the writer
            _writer = new ListBoxStreamWriter(log_output);
            // Redirect the out Console stream

            // Close previous output stream and redirect output to standard output.
            Console.Out.Close();
            Console.SetOut(_writer);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void MenuItemResultBrowser_Click(object sender, RoutedEventArgs e)
        {
            new OpenResultBrowserKey().Execute(sender);
        }

        private void raw_files_btn_Click(object sender, RoutedEventArgs e)
        {
            var fsd = new FolderSelectDialog();
            if (fsd.ShowDialog(IntPtr.Zero))
            {
                raw_files_dir.Text = fsd.FileName;
            }
        }

        private void results_btn_Click(object sender, RoutedEventArgs e)
        {
            var fsd = new FolderSelectDialog();
            if (fsd.ShowDialog(IntPtr.Zero))
            {
                results_dir.Text = fsd.FileName;
            }
        }
        private void run_btn_Click(object sender, RoutedEventArgs e)
        {
            ProgramParams myParams = GetParamsFromScreen();
            if (!CheckParams(myParams)) return;

            if (mainThread == null)
            {
                mainProgramGUI = new Program();
                mainProgramGUI.programParams = myParams;
                mainThread = new Thread(new ThreadStart(mainProgramGUI.Run));
                mainThread.SetApartmentState(ApartmentState.STA);
            }

            if (run_btn_text.Text.Equals("Run") && mainProgramGUI != null)
            {

                mainThread.Start();
                dispatcherTimer.Start();

                #region Disabling some fields
                raw_files_dir.IsEnabled = false;
                results_dir.IsEnabled = false;
                raw_files_btn.IsEnabled = false;
                results_btn.IsEnabled = false;
                dataGridPurityCorrections.IsEnabled = false;
                comboBoxPurityDefaults.IsEnabled = false;
                TabItemAdvancedParams.IsEnabled = false;
                #endregion

                run_btn_text.Text = "Stop";
                //change button icon
                run_btn_img.Source = ToBitmapSource(TMTXL.Properties.Resources.button_cancel_little);
            }
            else if (mainProgramGUI != null)
            {
                DialogResult answer = System.Windows.Forms.MessageBox.Show("Are you sure you want to stop the process ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (answer == System.Windows.Forms.DialogResult.Yes)
                {
                    dispatcherTimer.Stop();
                    #region Enabling some fields
                    raw_files_dir.IsEnabled = true;
                    results_dir.IsEnabled = true;
                    raw_files_btn.IsEnabled = true;
                    results_btn.IsEnabled = true;
                    dataGridPurityCorrections.IsEnabled = true;
                    comboBoxPurityDefaults.IsEnabled = true;
                    TabItemAdvancedParams.IsEnabled = true;
                    #endregion

                    run_btn_text.Text = "Run";
                    //change button icon
                    run_btn_img.Source = ToBitmapSource(TMTXL.Properties.Resources.goBtn);

                    if (mainThread != null)//When xlThread is null in this point is because the user stops the process, but before clicking on Yes button, the search is finished.
                    {
                        mainThread.Join();
                        mainThread.Interrupt();
                        mainThread = null;
                    }
                }
            }
        }

        public static BitmapSource ToBitmapSource(System.Drawing.Bitmap source)
        {
            BitmapSource bitSrc = null;

            var hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }

            return bitSrc;
        }

        internal static class NativeMethods
        {
            [DllImport("gdi32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DeleteObject(IntPtr hObject);
        }

        /// <summary>
        /// Method responsible for getting purity corrections matrix from datagrid form
        /// </summary>
        /// <param name="dataGridPurityCorrections"></param>
        /// <returns></returns>
        private List<List<double>> GetPurityCorrectionsFromForm()
        {

            if (dataGridPurityCorrections.ItemsSource == null)
            {
                return null;
            }

            DataTable dt = ((DataView)dataGridPurityCorrections.ItemsSource).ToTable();
            List<List<double>> correction = new List<List<double>>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                List<double> row = new List<double>();
                for (int j = 1; j < dt.Columns.Count; j++)
                {
                    //if (dt.Columns.Count == 7 && j == 4)// TMT 18-plex
                    //{
                    //    //We need to add the monoisotopic mass as -1 a requirement
                    //    row.Add(-1);
                    //}
                    //else if (dt.Columns.Count == 5 && j == 3)
                    //{
                    if (j == 3)
                    {
                        //We need to add the monoisotopic mass as -1 a requirement
                        row.Add(-1);
                    }
                    double v = double.Parse(dt.Rows[i].ItemArray[j].ToString());
                    row.Add(v);
                }
                correction.Add(row);

            }

            return correction;
        }


        /// <summary>
        /// Method responsible for getting params from screen
        /// </summary>
        /// <returns></returns>
        private ProgramParams GetParamsFromScreen()
        {
            ProgramParams myParams = new ProgramParams();
            myParams.RawFilesDir = raw_files_dir.Text;
            myParams.IDdir = results_dir.Text;
            myParams.PurityCorrectionMatrix = GetPurityCorrectionsFromForm();
            myParams.ClassLabels = class_label_textbox.Text;
            myParams.ControlChannel = control_channel_combobox.SelectedIndex;
            try
            {
                myParams.IsobaricMassess = Regex.Split(isobaric_masses_label_textbox.Text, " ").Select(a => double.Parse(a)).ToList();
            }
            catch (Exception)
            {
            }
            myParams.NormalizeSpectra = (bool)checkbox_normalization.IsChecked;
            myParams.NormalizeSpectraIntraClass = (bool)checkbox_normalization_intra_class.IsChecked;
            myParams.Multinoch = (short)multinoch_combobox.SelectedIndex;
            myParams.ChemicalLabel = ((ComboBoxItem)comboBoxPurityDefaults.SelectedValue).Content.ToString();
            return myParams;
        }

        /// <summary>
        /// Method responsible for checking screen params
        /// </summary>
        /// <param name="myParams"></param>
        /// <returns></returns>
        private bool CheckParams(ProgramParams myParams)
        {
            if (String.IsNullOrEmpty(myParams.RawFilesDir))
            {
                Console.WriteLine("ERROR: 'Directory with RAW files' field is empty. Please, select one directory.");
                System.Windows.MessageBox.Show(
                        "'Directory with RAW files' field is empty. Please, select one directory.",
                        "Warning",
                        (MessageBoxButton)MessageBoxButtons.OK,
                        (MessageBoxImage)MessageBoxIcon.Warning);
                return false;
            }

            if (String.IsNullOrEmpty(myParams.IDdir))
            {
                Console.WriteLine("ERROR: 'Directory with identifications' field is empty. Please, select one directory.");
                System.Windows.MessageBox.Show(
                        "'Directory with identifications' field is empty. Please, select one directory.",
                        "Warning",
                        (MessageBoxButton)MessageBoxButtons.OK,
                        (MessageBoxImage)MessageBoxIcon.Warning);
                return false;
            }

            if (myParams.PurityCorrectionMatrix == null)
            {
                Console.WriteLine("ERROR: No labeling kit in the purity correction tab has been selected.");
                System.Windows.MessageBox.Show(
                        "Please select the labeling kit in the purity correction tab.",
                        "Warning",
                        (MessageBoxButton)MessageBoxButtons.OK,
                        (MessageBoxImage)MessageBoxIcon.Warning);
                return false;
            }

            if (String.IsNullOrEmpty(class_label_textbox.Text))
            {
                Console.WriteLine("ERROR: No class label has been defined.");
                System.Windows.MessageBox.Show(
                        "Please set the classes labels on 'Advanced Parameters' tab.",
                        "Warning",
                        (MessageBoxButton)MessageBoxButtons.OK,
                        (MessageBoxImage)MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void dataGridPurityCorrections_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void comboBoxPurityDefaults_DropDownClosed(object sender, EventArgs e)
        {
            DataTable dt = new DataTable("Purity Corrections");

            //if (comboBoxPurityDefaults.Text.Equals("TMT 18"))
            //{
            //    dt.Columns.Add("Label", typeof(string));
            //    dt.Columns.Add("% of -3", typeof(double));
            //    dt.Columns.Add("% of -2", typeof(double));
            //    dt.Columns.Add("% of -1", typeof(double));
            //    dt.Columns.Add("% of +1", typeof(double));
            //    dt.Columns.Add("% of +2", typeof(double));
            //    dt.Columns.Add("% of +3", typeof(double));
            //    //    dt.Columns.Add("Label", typeof(string));
            //    //    dt.Columns.Add("% of -2x13C", typeof(double));
            //    //    dt.Columns.Add("% of -13C-15N", typeof(double));
            //    //    dt.Columns.Add("% of -13C", typeof(double));
            //    //    dt.Columns.Add("% of -15N", typeof(double));
            //    //    dt.Columns.Add("% of +15N", typeof(double));
            //    //    dt.Columns.Add("% of +13C", typeof(double));
            //    //    dt.Columns.Add("% of +13C15N", typeof(double));
            //    //    dt.Columns.Add("% of +2x13C", typeof(double));
            //}
            //else
            //{
            dt.Columns.Add("Label", typeof(string));
            dt.Columns.Add("% of -2", typeof(double));
            dt.Columns.Add("% of -1", typeof(double));
            dt.Columns.Add("% of +1", typeof(double));
            dt.Columns.Add("% of +2", typeof(double));
            //}

            switch (comboBoxPurityDefaults.Text)
            {
                case "Select":
                    dataGridPurityCorrections.ItemsSource = null;
                    break;
                case "iTRAQ 4":

                    DataRow dr1 = dt.NewRow();
                    dr1.ItemArray = new object[] { "114", 0, 1, 5.9, 0.2 };
                    dt.Rows.Add(dr1);

                    DataRow dr2 = dt.NewRow();
                    dr2.ItemArray = new object[] { "115", 0, 2, 5.6, 0.1 };
                    dt.Rows.Add(dr2);

                    DataRow dr3 = dt.NewRow();
                    dr3.ItemArray = new object[] { "116", 0, 3, 4.5, 0.1 };
                    dt.Rows.Add(dr3);

                    DataRow dr4 = dt.NewRow();
                    dr4.ItemArray = new object[] { "117", 0.1, 4, 3.5, 0.1 };
                    dt.Rows.Add(dr4);

                    class_label_textbox.Text = "1 1 2 2";

                    control_channel_combobox.Items.Clear();
                    control_channel_combobox.Items.Add("114");
                    control_channel_combobox.Items.Add("115");
                    control_channel_combobox.Items.Add("116");
                    control_channel_combobox.Items.Add("117");
                    control_channel_combobox.SelectedIndex = 0;

                    isobaric_masses_label_textbox.Text = "114.1 115.1 116.1 117.1";
                    break;
                case "TMT 6":

                    DataRow dr6TMT126 = dt.NewRow();
                    dr6TMT126.ItemArray = new object[] { "126", 0, 0.1, 9.4, 0.6 };
                    dt.Rows.Add(dr6TMT126);

                    DataRow dr6TMT127 = dt.NewRow();
                    dr6TMT127.ItemArray = new object[] { "127", 0, 0.5, 6.7, 0 };
                    dt.Rows.Add(dr6TMT127);

                    DataRow dr6TMT128 = dt.NewRow();
                    dr6TMT128.ItemArray = new object[] { "128", 0.1, 1.1, 4.2, 0 };
                    dt.Rows.Add(dr6TMT128);

                    DataRow dr6TMT129 = dt.NewRow();
                    dr6TMT129.ItemArray = new object[] { "129", 0, 1.7, 4.1, 0 };
                    dt.Rows.Add(dr6TMT129);

                    DataRow dr6TMT130 = dt.NewRow();
                    dr6TMT130.ItemArray = new object[] { "130", 0, 2.8, 2.5, 0 };
                    dt.Rows.Add(dr6TMT130);

                    DataRow dr6TMT131 = dt.NewRow();
                    dr6TMT131.ItemArray = new object[] { "131", 0.1, 4.1, 4.7, 0.1 };
                    dt.Rows.Add(dr6TMT131);

                    class_label_textbox.Text = "1 1 1 2 2 2";

                    control_channel_combobox.Items.Clear();
                    control_channel_combobox.Items.Add("126");
                    control_channel_combobox.Items.Add("127");
                    control_channel_combobox.Items.Add("128");
                    control_channel_combobox.Items.Add("129");
                    control_channel_combobox.Items.Add("130");
                    control_channel_combobox.Items.Add("131");
                    control_channel_combobox.SelectedIndex = 0;

                    isobaric_masses_label_textbox.Text = "126.1277 127.1247 128.1344 129.1315 130.1411 131.1382";

                    break;

                case "TMT 10":

                    DataRow dr10TMT126 = dt.NewRow();
                    dr10TMT126.ItemArray = new object[] { "126", 0, 0, 6.7, 0 };
                    dt.Rows.Add(dr10TMT126);

                    DataRow dr10TMT127N = dt.NewRow();
                    dr10TMT127N.ItemArray = new object[] { "127N", 0, 0, 7.9, 0 };
                    dt.Rows.Add(dr10TMT127N);

                    DataRow dr10TMT127C = dt.NewRow();
                    dr10TMT127C.ItemArray = new object[] { "127C", 0, 0.6, 5.8, 0 };
                    dt.Rows.Add(dr10TMT127C);

                    DataRow dr10TMT128N = dt.NewRow();
                    dr10TMT128N.ItemArray = new object[] { "128N", 0, 0.9, 6.8, 0.3 };
                    dt.Rows.Add(dr10TMT128N);

                    DataRow dr10TMT128C = dt.NewRow();
                    dr10TMT128C.ItemArray = new object[] { "128C", 0, 1.5, 5.8, 0 };
                    dt.Rows.Add(dr10TMT128C);

                    DataRow dr10TMT129N = dt.NewRow();
                    dr10TMT129N.ItemArray = new object[] { "129N", 0, 1.6, 5.9, 0 };
                    dt.Rows.Add(dr10TMT129N);

                    DataRow dr10TMT129C = dt.NewRow();
                    dr10TMT129C.ItemArray = new object[] { "129C", 0, 2.6, 3.9, 0 };
                    dt.Rows.Add(dr10TMT129C);

                    DataRow dr10TMT130N = dt.NewRow();
                    dr10TMT130N.ItemArray = new object[] { "130N", 0, 3.2, 4.7, 0 };
                    dt.Rows.Add(dr10TMT130N);

                    DataRow dr10TMT130C = dt.NewRow();
                    dr10TMT130C.ItemArray = new object[] { "130C", 0, 3.4, 3.6, 0 };
                    dt.Rows.Add(dr10TMT130C);

                    DataRow dr10TMT131 = dt.NewRow();
                    dr10TMT131.ItemArray = new object[] { "131", 0, 3.2, 3.7, 0 };
                    dt.Rows.Add(dr10TMT131);

                    class_label_textbox.Text = "1 1 1 1 1 2 2 2 2 2";

                    control_channel_combobox.Items.Clear();
                    control_channel_combobox.Items.Add("126");
                    control_channel_combobox.Items.Add("127N");
                    control_channel_combobox.Items.Add("127C");
                    control_channel_combobox.Items.Add("128N");
                    control_channel_combobox.Items.Add("128C");
                    control_channel_combobox.Items.Add("129N");
                    control_channel_combobox.Items.Add("129C");
                    control_channel_combobox.Items.Add("130N");
                    control_channel_combobox.Items.Add("130C");
                    control_channel_combobox.Items.Add("131");
                    control_channel_combobox.SelectedIndex = 0;

                    isobaric_masses_label_textbox.Text = "126.127726 127.124761 127.131081 128.128116 128.134436 129.131471 129.137790 130.134825 130.141145 131.138180";

                    break;


                case "TMT 16":

                    DataRow dr16TMT126 = dt.NewRow();
                    dr16TMT126.ItemArray = new object[] { "126", 0, 0, 7.73, 0 };
                    dt.Rows.Add(dr16TMT126);

                    DataRow dr16TMT127N = dt.NewRow();
                    dr16TMT127N.ItemArray = new object[] { "127N", 0, 0, 7.46, 0 };
                    dt.Rows.Add(dr16TMT127N);

                    DataRow dr16TMT127C = dt.NewRow();
                    dr16TMT127C.ItemArray = new object[] { "127C", 0, 0.71, 6.62, 0 };
                    dt.Rows.Add(dr16TMT127C);

                    DataRow dr16TMT128N = dt.NewRow();
                    dr16TMT128N.ItemArray = new object[] { "128N", 0, 0.75, 6.67, 0 };
                    dt.Rows.Add(dr16TMT128N);

                    DataRow dr16TMT128C = dt.NewRow();
                    dr16TMT128C.ItemArray = new object[] { "128C", 0, 1.34, 5.31, 0 };
                    dt.Rows.Add(dr16TMT128C);

                    DataRow dr16TMT129N = dt.NewRow();
                    dr16TMT129N.ItemArray = new object[] { "129N", 0, 1.29, 5.48, 0 };
                    dt.Rows.Add(dr16TMT129N);

                    DataRow dr16TMT129C = dt.NewRow();
                    dr16TMT129C.ItemArray = new object[] { "129C", 0, 2.34, 4.87, 0 };
                    dt.Rows.Add(dr16TMT129C);

                    DataRow dr16TMT130N = dt.NewRow();
                    dr16TMT130N.ItemArray = new object[] { "130N", 0, 2.36, 4.57, 0 };
                    dt.Rows.Add(dr16TMT130N);

                    DataRow dr16TMT130C = dt.NewRow();
                    dr16TMT130C.ItemArray = new object[] { "130C", 0, 2.67, 3.85, 0 };
                    dt.Rows.Add(dr16TMT130C);

                    DataRow dr16TMT131N = dt.NewRow();
                    dr16TMT131N.ItemArray = new object[] { "131N", 0, 2.71, 3.73, 0 };
                    dt.Rows.Add(dr16TMT131N);

                    DataRow dr16TMT131C = dt.NewRow();
                    dr16TMT131C.ItemArray = new object[] { "131C", 0, 3.69, 2.77, 0 };
                    dt.Rows.Add(dr16TMT131C);

                    DataRow dr16TMT132N = dt.NewRow();
                    dr16TMT132N.ItemArray = new object[] { "132N", 0, 2.51, 2.76, 0 };
                    dt.Rows.Add(dr16TMT132N);

                    DataRow dr16TMT132C = dt.NewRow();
                    dr16TMT132C.ItemArray = new object[] { "132C", 0, 4.11, 1.63, 0 };
                    dt.Rows.Add(dr16TMT132C);

                    DataRow dr16TMT133N = dt.NewRow();
                    dr16TMT133N.ItemArray = new object[] { "133N", 0, 3.09, 1.58, 0 };
                    dt.Rows.Add(dr16TMT133N);

                    DataRow dr16TMT133C = dt.NewRow();
                    dr16TMT133C.ItemArray = new object[] { "133C", 0, 4.63, 0.88, 0 };
                    dt.Rows.Add(dr16TMT133C);

                    DataRow dr16TMT134 = dt.NewRow();
                    dr16TMT134.ItemArray = new object[] { "134", 0, 4.82, 0.86, 0 };
                    dt.Rows.Add(dr16TMT134);

                    class_label_textbox.Text = "1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2";

                    control_channel_combobox.Items.Clear();
                    control_channel_combobox.Items.Add("126");
                    control_channel_combobox.Items.Add("127N");
                    control_channel_combobox.Items.Add("127C");
                    control_channel_combobox.Items.Add("128N");
                    control_channel_combobox.Items.Add("128C");
                    control_channel_combobox.Items.Add("129N");
                    control_channel_combobox.Items.Add("129C");
                    control_channel_combobox.Items.Add("130N");
                    control_channel_combobox.Items.Add("130C");
                    control_channel_combobox.Items.Add("131N");
                    control_channel_combobox.Items.Add("131C");
                    control_channel_combobox.Items.Add("132N");
                    control_channel_combobox.Items.Add("132C");
                    control_channel_combobox.Items.Add("133N");
                    control_channel_combobox.Items.Add("133C");
                    control_channel_combobox.Items.Add("134");
                    control_channel_combobox.SelectedIndex = 0;

                    isobaric_masses_label_textbox.Text = "126.127726 127.124761 127.131081 128.128116 128.134436 129.131471 129.137790 130.134825 130.141145 131.138180 131.1445 132.14153 132.14785 133.14489 133.15121 134.14824";
                    break;

                case "TMT 18":

                    DataRow dr18TMT126 = dt.NewRow();
                    dr18TMT126.ItemArray = new object[] { "126", /*0,*/ 0, 0, 0.31, 9.09/*, 0.32*/ };
                    dt.Rows.Add(dr18TMT126);

                    DataRow dr18TMT127N = dt.NewRow();
                    dr18TMT127N.ItemArray = new object[] { "127N", /*0,*/ 0, 0.78, 0, 9.41/*, 0.33*/ };
                    dt.Rows.Add(dr18TMT127N);

                    DataRow dr18TMT127C = dt.NewRow();
                    dr18TMT127C.ItemArray = new object[] { "127C", /*0,*/ 0.93, 0, 0.35, 8.63/*, 0.27 */};
                    dt.Rows.Add(dr18TMT127C);

                    DataRow dr18TMT128N = dt.NewRow();
                    dr18TMT128N.ItemArray = new object[] { "128N", /*0,*/ 0.82, 0.65, 0, 8.13/*, 0.26*/ };
                    dt.Rows.Add(dr18TMT128N);

                    DataRow dr18TMT128C = dt.NewRow();
                    dr18TMT128C.ItemArray = new object[] { "128C", /*0,*/ 1.47, 0, 0.34, 6.91/*, 0.15*/ };
                    dt.Rows.Add(dr18TMT128C);

                    DataRow dr18TMT129N = dt.NewRow();
                    dr18TMT129N.ItemArray = new object[] { "129N", /*0,*/ 1.46, 1.28, 0, 6.86/*, 0.15 */};
                    dt.Rows.Add(dr18TMT129N);

                    DataRow dr18TMT129C = dt.NewRow();
                    dr18TMT129C.ItemArray = new object[] { "129C", /*0.51,*/ 2.74, 0, 0.36, 6.15/*, 0.11 */};
                    dt.Rows.Add(dr18TMT129C);

                    DataRow dr18TMT130N = dt.NewRow();
                    dr18TMT130N.ItemArray = new object[] { "130N", /*0.13,*/ 2.41, 0.27, 0, 5.58/*, 0.11*/ };
                    dt.Rows.Add(dr18TMT130N);

                    DataRow dr18TMT130C = dt.NewRow();
                    dr18TMT130C.ItemArray = new object[] { "130C", /*0.04,*/ 3.1, 0, 0.42, 4.82/*, 0.06*/ };
                    dt.Rows.Add(dr18TMT130C);

                    DataRow dr18TMT131N = dt.NewRow();
                    dr18TMT131N.ItemArray = new object[] { "131N", /*0.03,*/ 2.78, 0.63, 0, 4.57/*, 0.12 */};
                    dt.Rows.Add(dr18TMT131N);

                    DataRow dr18TMT131C = dt.NewRow();
                    dr18TMT131C.ItemArray = new object[] { "131C", /*0.08,*/ 3.9, 0, 0.47, 3.57/*, 0.04*/ };
                    dt.Rows.Add(dr18TMT131C);

                    DataRow dr18TMT132N = dt.NewRow();
                    dr18TMT132N.ItemArray = new object[] { "132N", /*0.15,*/ 3.58, 0.72, 0, 1.8/*, 0 */};
                    dt.Rows.Add(dr18TMT132N);

                    DataRow dr18TMT132C = dt.NewRow();
                    dr18TMT132C.ItemArray = new object[] { "132C", /*0.11,*/ 4.55, 0, 0.43, 1.86/*, 0 */};
                    dt.Rows.Add(dr18TMT132C);

                    DataRow dr18TMT133N = dt.NewRow();
                    dr18TMT133N.ItemArray = new object[] { "133N", /*0.07,*/ 3.14, 0.73, 0, 3.4/*, 0 */};
                    dt.Rows.Add(dr18TMT133N);

                    DataRow dr18TMT133C = dt.NewRow();
                    dr18TMT133C.ItemArray = new object[] { "133C", /*0.22,*/ 4.96, 0, 0.34, 1.03/*, 0.03*/ };
                    dt.Rows.Add(dr18TMT133C);

                    DataRow dr18TMT134N = dt.NewRow();
                    dr18TMT134N.ItemArray = new object[] { "134N",/* 0.3, */5.49, 0.62, 0, 1.14/*, 0 */};
                    dt.Rows.Add(dr18TMT134N);

                    DataRow dr18TMT134C = dt.NewRow();
                    dr18TMT134C.ItemArray = new object[] { "134C", /*0.14,*/ 5.81, 0, 0.31, 0/*, 0*/ };
                    dt.Rows.Add(dr18TMT134C);

                    DataRow dr18TMT135N = dt.NewRow();
                    dr18TMT135N.ItemArray = new object[] { "135N", /*0.19,*/ 5.42, 0.36, 0, 0/*, 0*/ };
                    dt.Rows.Add(dr18TMT135N);

                    class_label_textbox.Text = "1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2";

                    control_channel_combobox.Items.Clear();
                    control_channel_combobox.Items.Add("126");
                    control_channel_combobox.Items.Add("127N");
                    control_channel_combobox.Items.Add("127C");
                    control_channel_combobox.Items.Add("128N");
                    control_channel_combobox.Items.Add("128C");
                    control_channel_combobox.Items.Add("129N");
                    control_channel_combobox.Items.Add("129C");
                    control_channel_combobox.Items.Add("130N");
                    control_channel_combobox.Items.Add("130C");
                    control_channel_combobox.Items.Add("131N");
                    control_channel_combobox.Items.Add("131C");
                    control_channel_combobox.Items.Add("132N");
                    control_channel_combobox.Items.Add("132C");
                    control_channel_combobox.Items.Add("133N");
                    control_channel_combobox.Items.Add("133C");
                    control_channel_combobox.Items.Add("134N");
                    control_channel_combobox.Items.Add("134C");
                    control_channel_combobox.Items.Add("135N");
                    control_channel_combobox.SelectedIndex = 0;

                    isobaric_masses_label_textbox.Text = "126.127726 127.124761 127.131081 128.128116 128.134436 129.131471 129.137790 130.134825 130.141145 131.138180 131.1445 132.14153 132.14785 133.14489 133.15121 134.148245 134.154565 135.1516";
                    break;

            }

            dataGridPurityCorrections.ItemsSource = new DataView(dt);
        }

        private void CommandBindingOpen_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private async void CommandBindingOpen_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
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
                    IsobaricAnalyzerControl IsobaricAnalyzerControl = new IsobaricAnalyzerControl();

                    #region Disabling some fields
                    tabControl.SelectedIndex = 0;
                    main_params_groupbox.IsEnabled = false;
                    dataGridPurityCorrections.IsEnabled = false;
                    comboBoxPurityDefaults.IsEnabled = false;
                    TabItemAdvancedParams.IsEnabled = false;
                    #endregion

                    await Task.Run(
                                () =>
                                {
                                    Console.WriteLine();
                                    IsobaricAnalyzerControl.LoadResults(dlg.FileName);
                                });

                    #region Enabling some fields
                    main_params_groupbox.IsEnabled = true;
                    dataGridPurityCorrections.IsEnabled = true;
                    comboBoxPurityDefaults.IsEnabled = true;
                    TabItemAdvancedParams.IsEnabled = true;
                    #endregion

                    ResultsWin resultsWin = new ResultsWin();
                    resultsWin.Setup(IsobaricAnalyzerControl.resultsPackage, dlg.FileName);
                    resultsWin.ShowDialog();
                }
                catch (Exception exc)
                {
                    Console.WriteLine("ERROR: " + exc.Message);
                    System.Windows.Forms.MessageBox.Show("Failed to load!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
        }
    }

    public class OpenResultBrowserKey : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ResultsWin resultsWin = new ResultsWin();
            resultsWin.ShowDialog();
        }
    }

    public class OpenResultsBrowserCommandContext
    {
        public ICommand OpenResultsBrowserCommand
        {
            get
            {
                return new OpenResultBrowserKey();
            }
        }
    }
}
