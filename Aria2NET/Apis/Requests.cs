﻿using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aria2NET.Exceptions;

namespace Aria2NET.Apis;

internal class Requests
{
    private readonly HttpClient _httpClient;
    private readonly Store _store;
    private static readonly JsonSerializerOptions s_jsonSerializerOptions;

    static Requests()
    {
        s_jsonSerializerOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = Aria2NetJsonSerializerContext.Default
        };
    }

    public Requests(HttpClient httpClient, Store store)
    {
        _httpClient = httpClient;
        _store = store;
    }

    private async Task<String> Request(String method, String? secret, CancellationToken cancellationToken, params Object?[]? parameters)
    {
        var requestUrl = $"{_store.Aria2Url}";

        var request = new Request
        {
            Id = "aria2net",
            Jsonrpc = "2.0",
            Method = method,
            Parameters = new List<Object?>()
        };

        if (!String.IsNullOrWhiteSpace(secret))
        {
            request.Parameters.Add($"token:{secret}");
        }

        if (parameters != null && parameters.Length > 0)
        {
            foreach (var parameter in parameters.Where(m => m != null))
            {
                request.Parameters.Add(parameter);
            }
        }

        var jsonRequest = JsonSerializer.Serialize(request, typeof(Request), s_jsonSerializerOptions);

        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var retryCount = 0;
        while (true)
        {
            try
            {
                var response = await _httpClient.PostAsync(requestUrl, content, cancellationToken);

                var buffer = await response.Content.ReadAsByteArrayAsync();
                var text = Encoding.UTF8.GetString(buffer);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    text = null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var aria2Exception = ParseAria2Exception(text);

                    if (aria2Exception != null)
                    {
                        throw aria2Exception;
                    }

                    throw new Exception(text);
                }

                if (String.IsNullOrEmpty(text))
                {
                    throw new Exception("No response received");
                }

                return text!;
            }
            catch (Aria2Exception)
            {
                throw;
            }
            catch
            {
                if (retryCount < _store.RetryCount)
                {
                    retryCount++;

                    await Task.Delay(1000 * retryCount, cancellationToken);
                }
                else
                {
                    throw;
                }
            }
        }
    }
        
    private async Task<T> Request<T>(String url, CancellationToken cancellationToken, params Object?[] parameters)
        where T : class, new()
    {
        var result = await Request(url, _store.Secret, cancellationToken, parameters);

        try
        {
            return JsonSerializer.Deserialize<T>(result, s_jsonSerializerOptions) ?? throw new JsonException();
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Unable to deserialize Aria2 API response to {typeof(T).Name}. Response was: {result}", ex);
        }
    }
        
    public async Task GetRequestAsync(String url, CancellationToken cancellationToken)
    {
        await Request(url, _store.Secret, cancellationToken);
    }

    public async Task GetRequestAsync(String url, CancellationToken cancellationToken, params Object[] parameters)
    {
        await Request(url, _store.Secret, cancellationToken, parameters);
    }

    public async Task<T> GetRequestAsync<T>(String url, CancellationToken cancellationToken)
    {
        var aria2Result = await Request<RequestResult<T>>(url, cancellationToken);

        if (aria2Result.Result == null)
        {
            throw new Exception("No response data received");
        }

        return aria2Result.Result;
    }

    public async Task<T> GetRequestAsync<T>(String url, CancellationToken cancellationToken, params Object?[] parameters)
    {
        var aria2Result = await Request<RequestResult<T>>(url, cancellationToken, parameters);

        if (aria2Result.Result == null)
        {
            throw new Exception("No response data received");
        }

        return aria2Result.Result;
    }

    public async Task<List<Object>> MultiRequestAsync(CancellationToken cancellationToken, params Object?[] parameters)
    {
        var parameterList = new List<MulticallRequest>();

        foreach (var param in parameters)
        {
            if (param is not Object[] methodCall)
            {
                throw new Exception($"Parameter must be of type Object[]");
            }

            var actualParameters = new List<Object>
            {
                $"token:{_store.Secret}"
            };
            actualParameters.AddRange(methodCall.Skip(1));

            if (methodCall[0] is not String methodName)
            {
                throw new Exception("Invalid method name in the first object");
            }

            var multicallRequest = new MulticallRequest
            {
                MethodName = methodName,
                Parameters = actualParameters.ToList()
            };

            parameterList.Add(multicallRequest);
        }

        var requestResult = await Request("system.multicall", null, cancellationToken, parameterList);

        try
        {
            var result = JsonSerializer.Deserialize<RequestResult<List<List<Object>>>>(requestResult, s_jsonSerializerOptions) ?? throw new JsonException();

            if (result.Result == null)
            {
                throw new JsonException();
            }

            return result.Result.Select(m => m.First()).ToList();
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Unable to deserialize Aria2 API response. Response was: {requestResult}", ex);
        }
    }
        
    private static Aria2Exception? ParseAria2Exception(String? text)
    {
        try
        {
            if (text == null)
            {
                return null;
            }

            var requestError = JsonSerializer.Deserialize<RequestResult<RequestError>>(text, s_jsonSerializerOptions);

            if (requestError?.Error != null)
            {
                return new Aria2Exception(requestError.Error.Code, requestError.Error.Message);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}