using System.Collections.Generic;
using System.Linq;
using System.Text;
using ATI.Gaidai.Entities;
using ATI.Gaidai.Enums;
using ATI.Services.Common.Behaviors;
using Microsoft.Extensions.Options;

namespace ATI.Gaidai
{
    public class GaidaiConfigValidator
    {
        private readonly ScenarioOptions _scenarioOptions;

        public GaidaiConfigValidator(IOptions<ScenarioOptions> scenarioOptions)
        {
            _scenarioOptions = scenarioOptions.Value;
        }

        public OperationResult ValidateScenarioConfiguration()
        {
            var errorCollection = new StringBuilder();

            var checkCollectionValue = CheckCollectionValue();
            if (!string.IsNullOrEmpty(checkCollectionValue))
            {
                errorCollection.Append(checkCollectionValue);
            }

            var checkMethodContinuity = CheckMethodsContinuity();
            if (!string.IsNullOrEmpty(checkMethodContinuity))
            {
                errorCollection.Append(checkMethodContinuity);
            }

            var checkScenariosContinuity = CheckScenariosContinuity();
            if (!string.IsNullOrEmpty(checkScenariosContinuity))
            {
                errorCollection.Append(checkScenariosContinuity);
            }

            var checkUrlParametersPosition = CheckUrlParametersPosition();
            if (!string.IsNullOrEmpty(checkUrlParametersPosition))
            {
                errorCollection.Append(checkUrlParametersPosition);
            }

            var checkSourceParameters = CheckSourceParameters();
            if (!string.IsNullOrEmpty(checkSourceParameters))
            {
                errorCollection.Append(checkSourceParameters);
            }
            
            var checkMethodParameters = CheckMethodsParameters();
            if (!string.IsNullOrEmpty(checkMethodParameters))
            {
                errorCollection.Append(checkMethodParameters);
            }
            
            var checkScenarioResponse = CheckScenarioResponseContinuity();
            if (!string.IsNullOrEmpty(checkScenarioResponse))
            {
                errorCollection.Append(checkScenarioResponse);
            }

            if (errorCollection.Length != 0)
            {
                return new OperationResult(ActionStatus.ConfigurationError, errorCollection.ToString());
            }

            return OperationResult.Ok;
        }
        
        private string CheckCollectionValue()
        {
            var checkResultMessage = new StringBuilder();
            var scenariosWithNullCollection = _scenarioOptions.Scenarios
                .Where(scenario => scenario.Methods == null ||
                                   scenario.ResponseFieldTransformation == null).ToList();

            if (scenariosWithNullCollection.Count != 0)
            {
                checkResultMessage.Append($"Configuration contains scenarios with collection null value: {string.Join('|', scenariosWithNullCollection)};\n");
            }

            var methodsWithNullCollection = _scenarioOptions.Methods
                .Where(method => method.ResponseFieldTransformation == null)
                .Where(method => method.Parameters == null)
                .Where(method => method.AdditionalCookies == null)
                .Where(method => method.AdditionalHeaders == null)
                .Where(method => method.TransitCookies == null)
                .Where(method => method.TransitHeaders == null)
                .ToList();

            if (methodsWithNullCollection.Count != 0)
            {
                checkResultMessage.Append($"Configuration contains methods with collection null value: {string.Join('|', methodsWithNullCollection)};\n");
            }

            return checkResultMessage.ToString();

        }

        private string CheckMethodsParameters()
        {
            var badMethods = new List<string>();
                
            foreach (var method in _scenarioOptions.Methods)
            {
                var allBodyParameters =
                    method.Parameters.Where(parameter => parameter.Destination == ParameterDestination.Body);
                var unnamedBodyParametersCount = method.Parameters.Count(parameter => parameter.UnnamedBody);

                if (unnamedBodyParametersCount != 0 && allBodyParameters.Count() != 1)
                {
                    badMethods.Add(method.Id);
                }
            }
            
            if(badMethods.Count != 0)
                return $"Methods: {string.Join('|', badMethods)} contains more than one unnamed body parameter";

            return null;
        }
        
        private string CheckMethodsContinuity()
        {
            var scenarioMethods = _scenarioOptions.Scenarios.SelectMany(scenario => scenario.Methods).Distinct();
            var exceptMethods = scenarioMethods.Except(_scenarioOptions.Methods.Select(method => method.Id)).ToList();

            if (exceptMethods.Count != 0)
            {
                return $"Configuration does not contains methods: {string.Join('|', exceptMethods)};\n";
            }

            return null;
        }
        private string CheckScenariosContinuity()
        {
            var methodsServices = _scenarioOptions.Methods.Select(method => method.ServiceId);
            var exceptServices = methodsServices.Except(_scenarioOptions.Services.Select(service => service.Id)).ToList();
            if (exceptServices.Count != 0)
            {
                return $"Configuration does not contains services: {string.Join('|', exceptServices)};\n";
            }

            return null;
        }
        private string CheckUrlParametersPosition()
        {
            var badUrlParametersMethods =
                _scenarioOptions.Methods.Where(
                    method => method.Parameters.Any(
                        param => param.Destination == ParameterDestination.Url &&
                                 !param.UrlPosition.HasValue))
                    .Select(method => method.Id)
                    .ToList();

            if (badUrlParametersMethods.Count != 0)
            {
                return $"Methods: {string.Join('|', badUrlParametersMethods)} have url parameters without position;\n";
            }

            return null;
        }
        private string CheckSourceParameters()
        {
            var methodsWithSourceParams =
                _scenarioOptions.Methods.Where(
                    method => method.Parameters.Any(param => !string.IsNullOrEmpty(param.SourceMethod)));

            var methodsWithSourceParamsIds = methodsWithSourceParams.Select(method => method.Id);

            var scenariosWithSourceParams =
                _scenarioOptions.Scenarios.Where(
                    scenario => methodsWithSourceParamsIds.Intersect(scenario.Methods).Any()).ToList();

            var badScenariosWithSourceParams = scenariosWithSourceParams.Where(scenario => !scenario.IsDependent).ToList();

            var checkResultMessage = new StringBuilder();

            if (badScenariosWithSourceParams.Count != 0)
            {
                checkResultMessage.Append(
                    $"Scenarios: {string.Join('|', badScenariosWithSourceParams)} have method with source params, but they are not dependent;\n");
            }

            var badScenarioMethods = new List<string>();

            foreach (var scenario in scenariosWithSourceParams)
            {
                var currentScenarioMethods = _scenarioOptions.Methods.Where(method => scenario.Methods.Contains(method.Id));
                var dependentMethods = currentScenarioMethods.Where(method =>
                    method.Parameters.Any(param => !string.IsNullOrEmpty(param.SourceMethod)));

                var currentScenarioIsBad = false;

                foreach (var dependentMethod in dependentMethods)
                {
                    foreach (var dependentMethodParameter in dependentMethod.Parameters.Where(param => !string.IsNullOrEmpty(param.SourceMethod)))
                    {
                        if (scenario.Methods.IndexOf(dependentMethod.Id) <
                            scenario.Methods.IndexOf(dependentMethodParameter.SourceMethod))
                        {
                            badScenarioMethods.Add(scenario.Id);
                            currentScenarioIsBad = true;
                            break;
                        }
                    }
                    if (currentScenarioIsBad)
                        break;
                }
            }

            if (badScenarioMethods.Count != 0)
            {
                checkResultMessage.Append(
                    $"Scenarios: {string.Join('|', badScenarioMethods)} have dependent methods with wrong sequence");
            }
            return checkResultMessage.ToString();
        }

        private string CheckScenarioResponseContinuity()
        {
            var badScenarioWithResponse =
                _scenarioOptions.Scenarios
                    .Where(scenario =>
                        scenario.ResponseMethods.Except(scenario.Methods).Count() != 0).ToList();

            var checkResponseMessage = new StringBuilder();

            if (badScenarioWithResponse.Count != 0)
            {
                checkResponseMessage.Append($"Scenarios: {string.Join('|', badScenarioWithResponse)} have not response method in scenario;\n");
            }

            return checkResponseMessage.ToString();
        }
    }
}

