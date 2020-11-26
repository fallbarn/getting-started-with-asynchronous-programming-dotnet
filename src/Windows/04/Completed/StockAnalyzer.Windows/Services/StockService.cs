using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
                var result = await client.GetAsync($"http://localhost:61363/api/stocks/{ticker}",
                    cancellationToken);

                result.EnsureSuccessStatusCode();

                var content = await result.Content.ReadAsStringAsync();

                // sle note: (the difficult bit to understand) although the code between the various awaits is suspended as a continuation, the main thread (UI in this case) still returns the Task to the calling level in
                // in the stack below. In this case the task is placed into a list of tasks that is collectively awaited on.
                return JsonConvert.DeserializeObject<IEnumerable<StockPrice>>(content);
            }
        }
    }

    public class MockStockService : IStockService
    {
        public Task<IEnumerable<StockPrice>> GetStockPricesFor(string ticker,
            CancellationToken cancellationToken)
        {
            var stocks = new List<StockPrice> {
                new StockPrice { Ticker = "MSFT", Change = 0.5m, ChangePercent = 0.75m },
                new StockPrice { Ticker = "MSFT", Change = 0.2m, ChangePercent = 0.15m },
                new StockPrice { Ticker = "GOOGL", Change = 0.3m, ChangePercent = 0.25m },
                new StockPrice { Ticker = "GOOGL", Change = 0.5m, ChangePercent = 0.65m }
            };

            return Task.FromResult(stocks.Where(stock => stock.Ticker == ticker));
        }
    }
}
