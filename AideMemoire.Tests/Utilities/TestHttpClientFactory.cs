using System.Net;

namespace AideMemoire.Tests.Utilities;

public class TestHttpClientFactory : IHttpClientFactory, IDisposable {
    private TestHttpMessageHandler handler = new();

    public HttpClient CreateClient(string name = "") {
        if (name != string.Empty) throw new NotImplementedException("Named clients are not supported in this test factory.");

        return new HttpClient(handler);
    }

    /// <summary>
    ///     Sets up a response for a specific URL
    /// </summary>
    public void Setup(string url, HttpStatusCode statusCode = HttpStatusCode.OK, string? content = null, string contentType = "text/html") {
        handler.Setup(url, statusCode, content, contentType);
    }

    /// <summary>
    ///     Sets up an exception to be thrown for a specific URL
    /// </summary>
    public void SetupException(string url, Exception exception) {
        handler.SetupException(url, exception);
    }

    /// <summary>
    ///     Gets all requests that were made
    /// </summary>
    public IReadOnlyList<HttpRequestMessage> Requests => handler.Requests;

    /// <summary>
    ///     Gets the request made to a specific URL
    /// </summary>
    public HttpRequestMessage GetRequestTo(string url) => GetRequestsTo(url).Single();

    /// <summary>
    ///     Gets requests made to a specific URL
    /// </summary>
    public IEnumerable<HttpRequestMessage> GetRequestsTo(string url) => handler.GetRequestsTo(url);

    /// <summary>
    ///     Verifies that a request was made to a specific URL
    /// </summary>
    public void VerifyRequest(string url, int expectedCount = 1) => handler.VerifyRequest(url, expectedCount);

    void IDisposable.Dispose() => handler.Dispose();
}
