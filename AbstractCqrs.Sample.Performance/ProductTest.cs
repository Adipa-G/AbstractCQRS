using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using AbstractCqrs.Sample.Domain.Product.Model;
using Newtonsoft.Json;

namespace AbstractCqrs.Sample.Performance.PerformanceTest
{
    public class ProductTest
    {
        private const int threads = 12;
        private readonly string urlBase;

        public ProductTest(string urlBase)
        {
            this.urlBase = urlBase;
        }

        public void Execute()
        {
            for (var i = 0; i < threads; i++)
            {
                var thread = new Thread(RunInThread);
                thread.Start();
            }
        }

        public void RunInThread()
        {
            var client = new HttpClient();
            var random = new Random();

            for (var i = 0; i < 100; i++)
            {
                var sameRepeat = random.Next(1, 10);
                var id = Guid.NewGuid();
                var deleted = false;

                for (var j = 0; j < sameRepeat; j++)
                {
                    if (deleted)
                    {
                        continue;
                    }

                    var product = new ProductModel
                                  {
                                      Id = id,
                                      Code = "Code " + i,
                                      Name = "Name " + i,
                                      UnitPrice = i
                                  };

                    if (j == 0)
                    {
                        var task = client.PostAsync($"{urlBase}/api/product",
                            new StringContent(
                                JsonConvert.SerializeObject(product),
                                Encoding.UTF8, "application/json"));
                        task.Wait();

                        var resultTask = task.Result.Content.ReadAsStringAsync();
                        resultTask.Wait();

                        var result = JsonConvert.DeserializeObject<ProductModel>(resultTask.Result);
                        id = result.Id;
                    }
                    else if (j < 9)
                    {
                        client.PutAsync($"{urlBase}/api/product/{id}",
                            new StringContent(
                                JsonConvert.SerializeObject(product),
                                Encoding.UTF8, "application/json")).Wait();
                    }
                    else
                    {
                        client.DeleteAsync($"{urlBase}/api/product/{id}")
                            .Wait();
                        deleted = true;
                    }
                }
            }
        }
    }
}