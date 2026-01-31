// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net;

namespace ObsidianTaskNotesExtension.Tests.Helpers;

/// <summary>
/// A mock HttpMessageHandler for testing HTTP client calls.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
  private readonly Dictionary<string, (HttpStatusCode StatusCode, string Content)> _responses = new();
  private readonly List<HttpRequestMessage> _requests = new();

  public IReadOnlyList<HttpRequestMessage> Requests => _requests.AsReadOnly();

  public void SetupResponse(string urlPattern, HttpStatusCode statusCode, string content)
  {
    _responses[urlPattern] = (statusCode, content);
  }

  public void SetupResponse(HttpStatusCode statusCode, string content)
  {
    _responses["*"] = (statusCode, content);
  }

  protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    _requests.Add(request);

    var url = request.RequestUri?.ToString() ?? "";

    // Try to find a matching response
    foreach (var kvp in _responses)
    {
      if (kvp.Key == "*" || url.Contains(kvp.Key))
      {
        return Task.FromResult(new HttpResponseMessage(kvp.Value.StatusCode)
        {
          Content = new StringContent(kvp.Value.Content)
        });
      }
    }

    // Default to 404 if no match
    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
    {
      Content = new StringContent("{\"success\": false, \"error\": \"Not found\"}")
    });
  }
}
