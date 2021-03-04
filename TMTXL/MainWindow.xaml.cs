using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
                    resultsWin.Setup(mainProgramGUI.tandemMassSpectra);
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
        /// Method responsible for getting params from screen
        /// </summary>
        /// <returns></returns>
        private ProgramParams GetParamsFromScreen()
        {
            ProgramParams myParams = new ProgramParams();
            myParams.RawFilesDir = raw_files_dir.Text;
            myParams.IDdir = results_dir.Text;

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

            return true;
        }
    }
}
