using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Net;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Click Events

        private void ExecuteSync_Click(object sender, RoutedEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            RunDownloadSync();

            var elapsedTimeMs = watch.ElapsedMilliseconds;
            resultsWindow.Text += $"Total execution time {elapsedTimeMs} (ms).";

            watch.Stop();
        }


        private async void ExecuteAsync_Click(object sender, RoutedEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            await RunDownloadAsync();

            var elapsedTimeMs = watch.ElapsedMilliseconds;
            resultsWindow.Text += $"Total execution time {elapsedTimeMs} (ms).";

            watch.Stop();
        }

        private async void ExecuteAsyncTaskList_Click(object sender, RoutedEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            await RunDownloadParallelAsync();

            var elapsedTimeMs = watch.ElapsedMilliseconds;
            resultsWindow.Text += $"Total execution time {elapsedTimeMs} (ms).";

            watch.Stop();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get a list of web sites
        /// </summary>
        /// <returns></returns>
        private List<string> GetPrepDataList()
        {
            resultsWindow.Text = "";

            List<string> output = new List<string>(new string[]
                {
                    "https://www.youtube.com/",
                    "https://www.microsoft.com/",
                    "https://www.ibm.com/",
                    "https://www.apple.com/" ,
                    "https://uk.yahoo.com/"
                });

            return output;
        }
        /// <summary>
        /// Download websites synchronously
        /// </summary>
        private void RunDownloadSync()
        {
            List<string> websiteList = GetPrepDataList();

            foreach (var site in websiteList)
            {
                WebsiteDataModel results = DownloadWebsite(site);
                ReportWebsiteInfo(results);
            }
        }
        /// <summary>
        /// Download websites async but in sequence - not locking up UI but they run in order from first to last.
        /// </summary>
        /// <returns></returns>
        private async Task RunDownloadAsync()
        {
            List<string> websiteList = GetPrepDataList();

            foreach (var site in websiteList)
            {
                WebsiteDataModel results = await Task.Run(() => DownloadWebsite(site));
                ReportWebsiteInfo(results);
            }
        }

        /// <summary>
        /// Downloads all websites at once in a single task. Does not lock up the UI.
        /// </summary>
        /// <returns></returns>
        private async Task RunDownloadParallelAsync()
        {
            List<string> websiteList = GetPrepDataList();

            List<Task<WebsiteDataModel>> tasks = new List<Task<WebsiteDataModel>>();

            foreach (var site in websiteList)
            {
                tasks.Add(DownloadWebsiteAsync(site));
                
                // If Method does not have a async method for downloading, 
                // then you can add Task.Run as commented out below, to wrap a sync
                // event in a task to make it async.

                //tasks.Add(Task.Run(() => DownloadWebsite(site)));
            }

            var result = await Task.WhenAll(tasks);

            foreach (var item in result)
            {
               ReportWebsiteInfo(item);
            }
        }
        /// <summary>
        /// Write our results to UI
        /// </summary>
        /// <param name="results"></param>
        private void ReportWebsiteInfo(WebsiteDataModel results)
        {
            resultsWindow.Text += $"{results.WebsiteUrl} downloaded {results.WebsiteData.Length} characters. \n";
        }
        
        /// <summary>
        /// Download website 
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private WebsiteDataModel DownloadWebsite(string site)
        {
            WebsiteDataModel output = new WebsiteDataModel();
            WebClient client = new WebClient();
            output.WebsiteUrl = site;
            output.WebsiteData = client.DownloadString(site);
            return output;
        }

        /// <summary>
        /// Download website 
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private async Task<WebsiteDataModel> DownloadWebsiteAsync(string site)
        {
            WebsiteDataModel output = new WebsiteDataModel();
            WebClient client = new WebClient();
            output.WebsiteUrl = site;
            output.WebsiteData = await client.DownloadStringTaskAsync(site);
            return output;
        }

        #endregion
    }

    #region Data Model

    internal class WebsiteDataModel
    {
        public string WebsiteUrl { get; set; }
        public string WebsiteData { get; set; }
    }

    #endregion
}
