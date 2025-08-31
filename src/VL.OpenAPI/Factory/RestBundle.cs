using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.OpenAPI
{
    public class RestBundle
    {
        public bool Execute = false;
        private RestSharp.RestRequest request;
        RestSharp.RestResponse response;
        RestClient client;
        public RestBundle()
        {
            request = new RestSharp.RestRequest();
            response = new RestSharp.RestResponse();
        }

        public void SetRequest(RestRequest request)
        {
            this.request = request;
        }

        public RestRequest GetRequest()
        {
            return this.request;
        }


        public void Update(RestClient restClient) 
        {
            if (this.Execute) 
            { 
                this.response = restClient.Execute(this.request);   
            }
        
        }

        public RestResponse GetResponse()
        {
            return response;
        }


        public void Split(out RestRequest Request, out string Response, out bool Execute)
        {
            Request = this.request;
            Response = this.response.Content;
            Execute = this.Execute;
        }
       
    }
}
