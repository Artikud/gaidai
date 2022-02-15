using System.Linq;
using System.Threading.Tasks;
using ATI.Gaidai.Entities;
using ATI.Gaidai.Helpers;
using ATI.Services.Common.Behaviors;
using ATI.Services.Common.Behaviors.OperationBuilder.Extensions;
using ATI.Services.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;


namespace ATI.Gaidai.Controllers
{
    [Route("v1/scenarios")]
    [ApiController]
    public class ScenariosController : Controller
    {
        private readonly ScenarioOptions _scenarioOptions;
        private readonly ScenarioHelper _scenarioHelper;


        public ScenariosController(IOptionsMonitor<ScenarioOptions> scenarioOptions, ScenarioHelper scenarioHelper)
        {
            _scenarioHelper = scenarioHelper;
            _scenarioOptions = scenarioOptions.CurrentValue;
        }

        [HttpPost("{scenarioId}")]
        public Task<IActionResult> ExecuteScenarioAsync(string scenarioId, [FromBody] JObject parameters)
        {
            var scenario = _scenarioOptions.Scenarios.FirstOrDefault(scene => scene.Id == scenarioId);

            if (scenario == null)
            {
                return Task.FromResult(CommonBehavior.GetActionResult(ActionStatus.BadRequest, false,
                    $"Scenario {scenarioId} not found"));
            }

            parameters ??= new JObject();
            parameters.Add("ScenarioId", JToken.FromObject(scenarioId));

            var requestParameters = new RequestParameters
            {
                Parameters = parameters,
                HeaderDictionary = HttpContext.Request.Headers,
                Cookies = HttpContext.Request.Cookies
            };

            if (scenario.IsForgettable)
            {
                _scenarioHelper.ExecuteScenarioAsync(scenario, requestParameters).Forget();

                return Task.FromResult(CommonBehavior.GetActionResult(ActionStatus.Ok, false));

            }

            return _scenarioHelper.ExecuteScenarioAsync(scenario, requestParameters).AsActionResultAsync();

        }


    }
}