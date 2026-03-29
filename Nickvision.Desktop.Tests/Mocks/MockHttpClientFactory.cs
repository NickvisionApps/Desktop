using System.Collections.Generic;
using System.Net.Http;

namespace Nickvision.Desktop.Tests.Mocks;

public class MockHttpClientFacotry : IHttpClientFactory
{
    private static readonly Dictionary<string, HttpClient> Clients;

    static MockHttpClientFacotry()
    {
        Clients = [];
    }

    public HttpClient CreateClient(string name)
    {
        if (Clients.TryGetValue(name, out var client))
        {
            return client;
        }
        Clients[name] = new HttpClient();
        return Clients[name];
    }
}
