using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ATI.Gaidai
{
    public static class JsonExtensions
    {
        public static bool TryParseToJson(this string input, out JObject result)
        {
            result = null;
            try
            {
                result = JObject.Parse(input);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static void RewriteFields(this JObject jObject, Dictionary<string, string> fieldsTransformation)
        {
            foreach (var fieldTransform in fieldsTransformation)
            {
                var selectToken = jObject.SelectToken(fieldTransform.Key);

                if (selectToken != null)
                {
                    selectToken.Parent.Remove();

                    var destinationPathArray = fieldTransform.Value.Split('.', StringSplitOptions.RemoveEmptyEntries).ToList();
                    var json = GetJson(destinationPathArray, selectToken);
                    var newJObject = JObject.Parse(json);
                    jObject.Merge(newJObject);
                }
            }
        }

        private static string GetJson(List<string> path, JToken value)
        {
            var result = string.Empty;
            if (path.Count != 0)
            {
                return $"{{\"{path.First()}\":{GetJson(path.Skip(1).ToList(), value)}}}";
            }
            result += $"\"{value}\"";
            return result;
        }

        private static string SourceParametersAdditionalKey = "_gaidai_rs_1923_";

        public static void AddParameterFromMethodResponse(this JObject jObject, string mainKey, JObject value)
        {
            jObject.Add(SourceParametersAdditionalKey + mainKey, value);
        }

        public static JObject GetParameterFromMethodResponse(this JObject jObject, string mainKey)
        {
            return jObject.GetValue(SourceParametersAdditionalKey + mainKey) as JObject;
        }

        public static JToken GetParameterTokenFromMethodResponse(this JObject jObject, string mainKey)
        {
            return jObject.SelectToken(SourceParametersAdditionalKey + mainKey);
        }
    }
}
