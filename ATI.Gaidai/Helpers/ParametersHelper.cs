using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using ATI.Gaidai.Entities;
using ATI.Gaidai.Enums;
using ATI.Services.Common.Behaviors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ATI.Gaidai.Helpers
{
    public class ParametersHelper
    {
        public OperationResult FillParameters(HttpRequestMessage requestMessage, Method method, JObject requestParameters, ref string methodPathAndQuery)
        {
            var parameterGroups = method.Parameters.ToLookup(val => val.Destination);
            StringContent requestContent = null;

            foreach (var parameterGroup in parameterGroups)
            {
                OperationResult fillParameterResult;
                switch (parameterGroup.Key)
                {
                    case ParameterDestination.Url:
                        fillParameterResult = FillUrlParameters(parameterGroup.ToList(), requestParameters, method.Url, ref methodPathAndQuery);
                        break;

                    case ParameterDestination.Header:
                        fillParameterResult = FillHeaderParameters(parameterGroup.ToList(), requestParameters, requestMessage);
                        break;

                    case ParameterDestination.Body:
                        fillParameterResult = FillBodyParameters(parameterGroup.ToList(), requestParameters, ref requestContent);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (!fillParameterResult.Success)
                {
                    return fillParameterResult;
                }
            }

            requestMessage.Content = requestContent;

            return OperationResult.Ok;
        }

        private OperationResult FillUrlParameters(List<MethodParameter> urlParameters, JObject requestParameters,
            string methodUrlMask, ref string methodPathAndQuery)
        {
            var sortedParams = new object[urlParameters.Count];

            foreach (var uriParameter in urlParameters)
            {
                var paramValue = GetRequestParameter(uriParameter, requestParameters);

                if (!paramValue.Success)
                {
                    return new OperationResult(paramValue);
                }
                sortedParams[uriParameter.UrlPosition ?? 0] = paramValue.Value;
            }

            methodPathAndQuery = string.Format(methodUrlMask, sortedParams);

            return OperationResult.Ok;
        }


        private OperationResult FillHeaderParameters(List<MethodParameter> headerParameters, JObject requestParameters, HttpRequestMessage requestMessage)
        {
            foreach (var headerParameter in headerParameters)
            {
                var paramValue = GetRequestParameter(headerParameter, requestParameters);

                if (!paramValue.Success)
                {
                    return new OperationResult(paramValue);
                }

                requestMessage.Headers.Add(headerParameter.Name, paramValue.Value.ToString());
            }
            return OperationResult.Ok;
        }


        private OperationResult FillBodyParameters(List<MethodParameter> bodyParameters, 
                                                   JObject requestParameters, 
                                                   ref StringContent requestContent)
        {

            var unnamedParameter =
                bodyParameters.Where(param => param.UnnamedBody).ToList();

            var buildOperation =
                unnamedParameter.Count!=0
                    ? BuildBodyStraightFromObject(unnamedParameter, requestParameters)
                    : BuildBodyFromParts(bodyParameters, requestParameters);

            if (!buildOperation.Success)
            {
                return buildOperation;
            }

            requestContent = buildOperation.Value;
            return OperationResult.Ok;
        }

        private OperationResult<StringContent> BuildBodyStraightFromObject(List<MethodParameter> unnamedMethodParams,
                                                                           JObject requestParameters)
        {
            var paramValueOperation = GetRequestParameter(unnamedMethodParams.FirstOrDefault(), requestParameters);

            if (!paramValueOperation.Success)
                return new OperationResult<StringContent>(paramValueOperation);

            return new OperationResult<StringContent>(new StringContent(JsonConvert.SerializeObject(paramValueOperation.Value),
                                                                        Encoding.UTF8, 
                                                                        "application/json"));
        }

        private OperationResult<StringContent> BuildBodyFromParts(List<MethodParameter> bodyParameters, JObject requestParameters)
        {
            var bodyBuilder = new JObject();
            foreach (var bodyParameter in bodyParameters)
            {
                var paramValue = GetRequestParameter(bodyParameter, requestParameters);

                if (!paramValue.Success)
                {
                    return new OperationResult<StringContent>(paramValue);
                }

                bodyBuilder.Add(bodyParameter.Name, paramValue.Value.ToString());
            }
            
            return new OperationResult<StringContent>(new StringContent(bodyBuilder.ToString(), Encoding.UTF8, "application/json"));
        }
        
        private OperationResult<JToken> GetRequestParameter(MethodParameter parameter, JObject requestParameters)
        {
            JToken paramValue;
            if (!string.IsNullOrEmpty(parameter.SourceMethod))
            {
                paramValue = requestParameters.GetParameterTokenFromMethodResponse($"{parameter.SourceMethod}.{parameter.Name}");

                if (paramValue == null && parameter.Required)
                {
                    return new OperationResult<JToken>(ActionStatus.BadRequest,
                        $"Preview method does not contains required {parameter.Name} parameter in answer");
                }
            }
            else
            {
                paramValue = requestParameters.GetValue(parameter.Name);
                if (paramValue == null && parameter.Required)
                {
                    return new OperationResult<JToken>(ActionStatus.BadRequest,
                        $"Request does not contains required {parameter.Name} parameter");
                }
            }

            return new OperationResult<JToken>(paramValue);
        }



    }
}
