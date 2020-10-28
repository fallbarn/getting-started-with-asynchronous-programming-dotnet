using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Newtonsoft.Json;
using StockAnalyzer.Core.Domain;

namespace StockAnalyzer.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            #region Before loading stock data
            var watch = new Stopwatch();
            watch.Start();
            StockProgress.Visibility = Visibility.Visible;
            StockProgress.IsIndeterminate = true;
            #endregion

            try
            {
                await GetStocks();
            }
            catch (Exception ex)
            {
                Notes.Text += ex.Message;
            }

            #region After stock data is loaded
            StocksStatus.Text = $"Loaded stocks for {Ticker.Text} in {watch.ElapsedMilliseconds}ms";
            StockProgress.Visibility = Visibility.Hidden;
            #endregion
        }

        // sle note: Methods with async must return a task type. In this case there is no parameter
        // Once prefixed, the method can call other async methods (using the await) and ensure the the UI message queue will be pumped, that that
        // the preceeding code will be run as a continuation.
        public async Task GetStocks()
        {
            using (var client = new HttpClient())
            {
                // sle note: await causes the UI thread to pump the windows messages, but holding the thread ready to complete the method. It also return the Type of T ( in this case HttpResponseMessage) from the generic method              
                var response = await client.GetAsync($"http://localhost:61363/api/stocks/{Ticker.Text}");

                try
                {
                    // sle note: throws an exception if there is an error in the HTTPResponseMessage. To be handled in the exception handler.
                    // This is perculiar to the HTTPResponseMessage
                    response.EnsureSuccessStatusCode();

                    // sle note: Returns a Task<string>. The await pumps the UI, and finally returns the <string> from the task.
                    var content = await response.Content.ReadAsStringAsync();

                    var data = JsonConvert.DeserializeObject<IEnumerable<StockPrice>>(content);

                    Stocks.ItemsSource = data;
                }
                catch (Exception ex)
                {
                    Notes.Text += ex.Message;
                }
            }
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));

            e.Handled = true;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}