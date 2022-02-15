using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ATI.Gaidai.Entities;
using ATI.Services.Common.Behaviors;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace ATI.Gaidai.Helpers
{
    public class ScenarioHelper
    {
        private readonly ScenarioOptions _scenarioOptions;
        private readonly HttpRequestHelper _httpRequestHelper;
        private readonly HttpClient _httpClient;

        public ScenarioHelper(IOptionsMonitor<ScenarioOptions> scenarioOptions, HttpClient httpClient, HttpRequestHelper httpRequestHelper)
        {
            _httpRequestHelper = httpRequestHelper;
            _scenarioOptions = scenarioOptions.CurrentValue;
            _httpClient = httpClient;
        }

        public async Task<OperationResult<JObject>> ExecuteScenarioAsync(Scenario scenario, RequestParameters requestParameters)
        {
            if (scenario.IsDependent)
            {
                return await ExecuteDependentScenario(scenario, requestParameters);
            }
            return await ExecuteIndependentScenario(scenario, requestParameters);
        }

        private async Task<OperationResult<JObject>> ExecuteDependentScenario(Scenario scenario, RequestParameters requestParameters)
        {
            foreach (var method in scenario.Methods)
            {
                var methodExecuteResult = await ExecuteMethodAsync(method, requestParameters);
                if (!methodExecuteResult.Success)
                {
                    return new OperationResult<JObject>(methodExecuteResult);
                }
            }

            return GetResponseBody(scenario, requestParameters);
        }
        private async Task<OperationResult<JObject>> ExecuteIndependentScenario(Scenario scenario, RequestParameters requestParameters)
        {
            var allMethodIsOk = true;
            var errorMethods = new List<string>();
            var tasks = scenario.Methods.Select(async method =>
            {
                var executeMethodResult = await ExecuteMethodAsync(method, requestParameters);
                if (!executeMethodResult.Success)
                {
                    allMethodIsOk = false;

                    if (executeMethodResult.ActionStatus == ActionStatus.InternalServerError)
                    {
                        errorMethods.Add(method);
                    }
                }
            });

            await Task.WhenAll(tasks);

            if (scenario.IsCommonSuccessful && !allMethodIsOk)
            {
                if (errorMethods.Count != 0)
                {
                    return new OperationResult<JObject>(ActionStatus.InternalServerError,
                        $"Error in methods: {string.Join(',', errorMethods)}");
                }
                return new OperationResult<JObject>(ActionStatus.BadRequest,
                    $"Bad request to methods: {string.Join(',', errorMethods)}");
            }

            return GetResponseBody(scenario, requestParameters);
        }
        private OperationResult<JObject> GetResponseBody(Scenario scenario, RequestParameters parameters)
        {
            var result = new JObject();

            if (scenario.ResponseMethods.Count != 0)
            {
                foreach (var responseMethod in scenario.ResponseMethods)
                {
                    if (scenario.IsRewritableResponse)
                    {
                        foreach (var property in parameters.Parameters.GetParameterFromMethodResponse(responseMethod).Properties())
                        {
                            if (result.ContainsKey(property.Name))
                            {
                                result[property.Name] = property.Value;
                            }
                            else
                            {
                                result.Add(property);
                            }
                        }
                    }
                    else
                    {
                        result.Add(responseMethod, parameters.Parameters.GetParameterFromMethodResponse(responseMethod));
                    }
                }
                result.RewriteFields(scenario.ResponseFieldTransformation);

                return new OperationResult<JObject>(result);
            }

            return new OperationResult<JObject>();
        }
        private async Task<OperationResult> ExecuteMethodAsync(string methodId, RequestParameters requestParameters)
        {
            var method = _scenarioOptions.Methods.FirstOrDefault(m => m.Id == methodId);

            if (method == null)
            {
                return new OperationResult(ActionStatus.BadRequest, $"Configuration does not contains {methodId} method");
            }

            var service = _scenarioOptions.Services.FirstOrDefault(s => s.Id == method.ServiceId);
            if (service == null)
            {
                return new OperationResult(ActionStatus.BadRequest, $"Configuration does not contains {method.ServiceId} service");
            }

            var operationResult = _httpRequestHelper.GetRequestMessage(service, method, requestParameters);

            if (!operationResult.Success)
            {
                return operationResult;
            }

            var result = await _httpClient.SendAsync(operationResult.Value);

            var resultString = await result.Content.ReadAsStringAsync();

            string errorReason = null;

            if (resultString.TryParseToJson(out var methodContent))
            {
                methodContent.RewriteFields(method.ResponseFieldTransformation);

                requestParameters.Parameters.AddParameterFromMethodResponse(methodId, methodContent);

                errorReason = method.ErrorReasonField != null
                    ? methodContent.GetValue(method.ErrorReasonField, StringComparison.InvariantCultureIgnoreCase)?.ToString()
                    : null;
            }

            if (result.IsSuccessStatusCode)
            {
                return OperationResult.Ok;
            }

            if ((int)result.StatusCode >= 400 && (int)result.StatusCode < 500)
            {
                return new OperationResult(ActionStatus.BadRequest, errorReason);
            }

            return new OperationResult(ActionStatus.InternalServerError, errorReason ?? $"Method {methodId} executed with exception");
        }

    }
}
