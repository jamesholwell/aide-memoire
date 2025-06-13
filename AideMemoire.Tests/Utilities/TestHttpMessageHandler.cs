using System.Net;
using System.Text;

namespace AideMemoire.Tests.Utilities;

public class TestHttpMessageHandler : HttpMessageHandler {
    private readonly Dictionary<string, HttpResponseMessage> _responses = [];

    private readonly Dictionary<string, Exception> _exceptions = [];

    private readonly List<HttpRequestMessage> _requests = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
        // store the request for verification
        _requests.Add(request);

        var url = request.RequestUri?.ToString() ?? "";

        // check if an exception should be thrown
        if (_exceptions.TryGetValue(url, out Exception? value))
            throw value;

        // check if a response is configured
        if (_responses.TryGetValue(url, out HttpResponseMessage? response)) {
            _responses.Remove(url); // remove to prevent reuse (if this is desired, implement as a dictionary of queues)
            return Task.FromResult(response);
        }

        // default response for unmatched URLs
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) {
            Content = new StringContent("Not Found", Encoding.UTF8, "text/plain")
        });
    }

    /// <summary>
    ///     Sets up a response for a specific URL
    /// </summary>
    public void Setup(string url, HttpStatusCode statusCode = HttpStatusCode.OK, string? content = null, string contentType = "text/html") {
        _responses[url] = new HttpResponseMessage(statusCode) {
            Content = content == null ? null : new StringContent(content, Encoding.UTF8, contentType)
        };
    }

    /// <summary>
    ///     Sets up an exception to be thrown for a specific URL
    /// </summary>
    public void SetupException(string url, Exception exception) {
        _exceptions[url] = exception;
    }

    /// <summary>
    ///     Gets all requests that were made
    /// </summary>
    public IReadOnlyList<HttpRequestMessage> Requests => _requests.AsReadOnly();

    /// <summary>
    ///     Gets the request made to a specific URL
    /// </summary>
    public HttpRequestMessage GetRequestTo(string url) {
        return GetRequestsTo(url).Single();
    }

    /// <summary>
    ///     Gets requests made to a specific URL
    /// </summary>
    public IEnumerable<HttpRequestMessage> GetRequestsTo(string url) {
        return _requests.Where(r => r.RequestUri?.ToString() == url);
    }

    /// <summary>
    ///     Verifies that a request was made to a specific URL
    /// </summary>
    public void VerifyRequest(string url, int expectedCount = 1) {
        var actualCount = GetRequestsTo(url).Count();
        if (actualCount != expectedCount) {
            throw new InvalidOperationException(
                $"Expected {expectedCount} request(s) to '{url}', but found {actualCount}");
        }
    }

    /// <summary>
    ///     Clears all setup and request history
    /// </summary>
    public void Reset() {
        foreach (var response in _responses.Values)
            response.Dispose();

        _requests.Clear();
        _exceptions.Clear();
        _responses.Clear();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            Reset();
        }

        base.Dispose(disposing);
    }
}
