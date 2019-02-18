using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace console1
{
  class Program
  {
    private const string ServiceAddress = "https://localhost:5001";

    static async Task Main(string[] args)
    {
      await ComposeManually();
      await ComposeWithContainer(args.FirstOrDefault() == "development");
    }

    private static async Task ComposeManually()
    {
      var handler = new HttpClientHandler()
      {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
      };

      var client = new HttpClient(handler);
      client.BaseAddress = new Uri(ServiceAddress);


      var response = await client.GetAsync("/");

      Console.WriteLine(await response.Content.ReadAsStringAsync());
    }

    static async Task ComposeWithContainer(bool development)
    {
      ServiceProvider serviceProvider = ConfigureServices(development);

      var httpClientfactory = serviceProvider.GetService<IHttpClientFactory>();

      var client = httpClientfactory.CreateClient("my-client");

      var response = await client.GetAsync("/");

      Console.WriteLine(await response.Content.ReadAsStringAsync());

    }

    private static ServiceProvider ConfigureServices(bool development)
    {
      IServiceCollection serviceCollection = new ServiceCollection();

      var clientBuilder = serviceCollection
        .AddHttpClient("my-client")
        .ConfigureHttpClient(myClient =>
        {
          myClient.BaseAddress = new Uri(ServiceAddress);
        });

      if (development)
      {
        clientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
          ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
      }
      var serviceProvider = serviceCollection.BuildServiceProvider();
      return serviceProvider;
    }
  }
}
