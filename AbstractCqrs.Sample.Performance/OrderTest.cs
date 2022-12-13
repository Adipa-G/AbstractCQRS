using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using AbstractCqrs.Sample.Domain.Order.Model;
using Newtonsoft.Json;

namespace AbstractCqrs.Sample.Performance.PerformanceTest
{
    public class OrderTest
    {
        private const int threads = 12;
        private readonly string urlBase;

        public OrderTest(string urlBase)
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
                var id = Guid.Empty;
                var deleted = false;

                for (var j = 0; j < sameRepeat; j++)
                {
                    if (deleted)
                    {
                        continue;
                    }

                    var order = new OrderModel()
                                  {
                                      Id = id,
                                      CustomerNotes =
                                          "Cust Note " + i + "_" + j,
                                      OrderNo = i + "_" + j,
                                      Discount = i,
                                      Total = i
                                  };

                    if (j == 0)
                    {
                        var task = client.PostAsync($"{urlBase}/api/order",
                            new StringContent(
                                JsonConvert.SerializeObject(order),
                                Encoding.UTF8, "application/json"));
                        task.Wait();

                        var resultTask = task.Result.Content.ReadAsStringAsync();
                        resultTask.Wait();

                        var result = JsonConvert.DeserializeObject<OrderModel>(resultTask.Result);
                        id = result.Id;
                    }
                    else if (j < 9)
                    {
                        client.PutAsync($"{urlBase}/api/order/{id}",
                            new StringContent(
                                JsonConvert.SerializeObject(order),
                                Encoding.UTF8, "application/json")).Wait();
                    }
                    else
                    {
                        client.DeleteAsync($"{urlBase}/api/order/{id}")
                            .Wait();
                        deleted = true;
                    }
                }

                if (!deleted)
                {
                    AddOrderItems(client, random, id);
                }
            }
        }

        private void AddOrderItems(HttpClient client,Random random,Guid orderId)
        {
            var count = random.Next(1, 10);
            for (int i = 0; i < count; i++)
            {
                var sameRepeat = random.Next(1, 10);
                var id = Guid.NewGuid();
                var productId = Guid.NewGuid();
                var deleted = false;

                for (var j = 0; j < sameRepeat; j++)
                {
                    if (deleted)
                    {
                        continue;
                    }

                    var orderItem = new OrderItemModel()
                                {
                                    Id = id,
                                    Amount = i * j,
                                    Ordinal = i,
                                    ProductText = "Product " +  productId,
                                    Qty = i,
                                    QtyUnit = "j_" + "Kg",
                                    ProductId = productId
                                };

                    if (j == 0)
                    {
                        var task = client.PostAsync($"{urlBase}/api/order/item/{orderId}",
                            new StringContent(
                                JsonConvert.SerializeObject(orderItem),
                                Encoding.UTF8, "application/json"));
                        task.Wait();

                        var resultTask = task.Result.Content.ReadAsStringAsync();
                        resultTask.Wait();

                        var result = JsonConvert.DeserializeObject<OrderItemModel>(resultTask.Result);
                        id = result.Id;
                    }
                    else if (j < 9)
                    {
                        client.PutAsync($"{urlBase}/api/order/item/{orderId}/{id}",
                            new StringContent(
                                JsonConvert.SerializeObject(orderItem),
                                Encoding.UTF8, "application/json")).Wait();
                    }
                    else
                    {
                        client.DeleteAsync($"{urlBase}/api/order/item/{orderId}/{id}")
                            .Wait();
                        deleted = true;
                    }
                }
            }
        }
    }
}