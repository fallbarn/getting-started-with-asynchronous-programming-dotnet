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
                // sle note: the 'await' auto strips the result from the task.
                // Awaitable methods should always return a task containing a result!
                var result = await client.GetAsync($"http://localhost:61363/api/stocks/{ticker}",
                    cancellationToken);

                // sle note: throws an exception if the web request does not succeed.
                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<IEnumerable<StockPrice>>(content);
            }
        }
    }

    public class MockStockService : IStockService
    {
        public Task<IEnumerable<StockPrice>> GetStockPricesFor(string ticker,
            CancellationToken cancellationToken)
        {

            //// sle note: simple aid-memoire task example. This task has a generic result of type T set to int.
            //var t = new Task<int>(()=> { return 1; });
            //t.Start();
            //Task.WaitAll(t);
            //var result = t.Result;

            //// sle note: no result for this example!
            //var t2 = new Task(() => { MessageBox.Show("No result parameter"); });
            //t2.Start();
            //Task.WaitAll(t2);

            //// sle note: alternative way to use tasks without the async keyword. Here the continuation is explicit.
            //Task.Run(() => { return 1; }).ContinueWith((x) => MessageBox.Show($"Result is equal to:{x.Result}"));



            //var stocks = new List<StockPrice> {
            //    new StockPrice { Ticker = "MSFT", Change = 0.5m, ChangePercent = 0.75m },
            //    new StockPrice { Ticker = "MSFT", Change = 0.2m, ChangePercent = 0.15m },
            //    new StockPrice { Ticker = "GOOGL", Change = 0.3m, ChangePercent = 0.25m },
            //    new StockPrice { Ticker = "GOOGL", Change = 0.5m, ChangePercent = 0.65m }
            //};

            var stocks = new List<StockPrice>();

            Task.Run(async() => {
                stocks.Add(new StockPrice { Ticker = "MSFT", Change = 0.5m, ChangePercent = 0.75m });
                stocks.Add(new StockPrice { Ticker = "MSFT", Change = 0.2m, ChangePercent = 0.15m });
                // Simulate a ten second  delay whilst loading thousands of other tickers
                await Task.Delay(10000);

                stocks.Add(new StockPrice { Ticker = "GOOGL", Change = 0.3m, ChangePercent = 0.25m });
                stocks.Add(new StockPrice { Ticker = "GOOGL", Change = 0.5m, ChangePercent = 0.65m });
            });

           

            return Task.FromResult(stocks.Where(stock => stock.Ticker == ticker));
        }
    }
}