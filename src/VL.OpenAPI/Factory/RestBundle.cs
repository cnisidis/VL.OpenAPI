using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VL.Lib.Collections;

namespace VL.OpenAPI
{
    public class RestBundle
    {
        public bool Execute = false;
        private RestSharp.RestRequest request;
        RestSharp.RestResponse response;
        RestClient client;
        private string _template;
        private bool _hasSegmetns = false;
        public RestBundle(string template, string method)
        {
            request = new RestSharp.RestRequest(template, RestBundle.ToRestMethod(method)) ;
            response = new RestSharp.RestResponse();
            _template = template;

           

            if(template.Contains("{"))
            {
                _hasSegmetns = true;
            }
        }

        public void InitParameters(Dictionary<string, string> parameters)
        {
            foreach(var p in parameters)
            {

            }
        }

        public void UpdateParameter(string parameterName, string value, bool encode = true)
        {
            if (_hasSegmetns)
            {
                this.request.AddUrlSegment(parameterName, value, encode);
            }
            else
            {
                this.request.AddOrUpdateParameter(parameterName, value, encode);
            }
        }
        public void SetRequest(RestRequest request)
        {
            this.request = request;
        }

        public RestRequest GetRequest()
        {
            return this.request;
        }

        public void SetTemplate(string template)
        {
            this._template = template;
        }

        public void Update(RestClient restClient) 
        {
            if (this.Execute && restClient != null) 
            { 
                this.response = restClient.Execute(this.request);   
            }
        
        }

        public RestResponse GetResponse()
        {
            return response;
        }

        public string GetResponseAsString()
        {
            return response.Content;
        }

        public void Split(out Method Method, out string Template, out Spread<KeyValuePair<string, object>> Parameters, out string Response, out bool HasSegments, out bool Execute)
        {
            Template = this._template;
            Response = this.response.Content;
            Execute = this.Execute;
            Parameters = this.GetParameters().ToSpread();
            HasSegments = _hasSegmetns;
            Method = this.request.Method;
        }

        public static RestRequest BuildRequest(HttpMethod httpMethod)
        {
            var method = (Method)Enum.Parse(typeof(Method), httpMethod.ToString(), true);
            var request = new RestSharp.RestRequest();

            return null;
        }


        public IEnumerable<KeyValuePair<string, object>> GetParameters()
        {
            var param = this.request.Parameters;
            foreach (var parameter in param)
            {
                yield return new KeyValuePair<string, object>(parameter.Name, parameter.Value);
            }
        }

        public static Method ToRestMethod(string method)
        {
            var m = method.ToLower();
            var firstLetter = m.Substring(0, 1).ToUpper();
            var rest = m.Substring(1);
            m = firstLetter + rest;

            Method parsed = Method.Get;
            Enum.TryParse<Method>(m, out parsed);

            //Console.WriteLine($"Parsing method : {method} --> {m} --> {parsed}");

            return parsed;
            
        }
       
    }
}
