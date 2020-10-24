using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using StockAnalyzer.Core.Domain;

namespace StockAnalyzer.Windows.Services
{
    public interface IStockService
    {
        Task<IEnumerable<StockPrice>> GetStockPricesFor(string ticker,
            CancellationToken cancellationToken);
    }

    public class StockService : IStockService
    {
        int i = 0;
        public async Task<IEnumerable<StockPrice>> GetStockPricesFor(string ticker,
            CancellationToken cancellationToken)
        {
            await Task.Delay((i++) * 1000);
            using (var client = new HttpClient())
            {
                // sle note: Make the call to service but use await to retain the UI responsiveness
                // Specifically, it allows the CancellationToken to be monitored in the UI thread.
                var result = await client.GetAsync($"http://localhost:61363/api/stocks/{ticker}",
                    cancellationToken);

                result.EnsureSuccessStatusCode();

                // sle note: Make the call to ReadAsString but use await to retain the UI responsiveness (this with a massive list might take sometime)
                var content = await result.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<IEnumerable<StockPrice>>(content);
            }
        }
    }

    public class MockStockService : IStockService
    {
        public async Task<IEnumerable<StockPrice>> GetStockPricesFor(string ticker,
            CancellationToken cancellationToken)
        {

            var stocks = new List<StockPrice>();

            try
            {
                //Thread.Sleep(5000); // sle note: doesn't let the thread return to the UI so the cancellation button locks
                await Task.Delay(5000);

                // the following becomes a continuation due to the above await


                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                stocks = new List<StockPrice> {
                    new StockPrice { Ticker = "MSFT", Change = 0.5m, ChangePercent = 0.75m },
                    new StockPrice { Ticker = "MSFT", Change = 0.2m, ChangePercent = 0.15m },
                    new StockPrice { Ticker = "GOOGL", Change = 0.3m, ChangePercent = 0.25m },
                    new StockPrice { Ticker = "GOOGL", Change = 0.5m, ChangePercent = 0.65m }
                };


                // sle note: Linq return IEnumerable, so must convert back to list.
                stocks = stocks.Where(stock => stock.Ticker == ticker).ToList();
            }
            catch (Exception ex)
            {
                
                MessageBox.Show(ex.Message);
                // sle note: shows now exceptions are swallowed within async operations
                throw;
            }

            var result = await Task.FromResult(stocks.Where(stock => stock.Ticker == ticker));

            // the following becomes a continuation due to the above await

            return result;
        }

    }

}
