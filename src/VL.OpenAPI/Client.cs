
using RestSharp;
using VL.Lib.Collections;

namespace VL.OpenAPI
{
    public class Client:IDisposable
    {
        RestSharp.RestClient client;
        private List<RestBundle> _restBundles;

        

        public Client(string apiToken, string baseUri)
        {
            RestClientOptions clientOptions = new RestClientOptions(baseUri) {  };
            client = new RestSharp.RestClient(clientOptions);
            _restBundles = new List<RestBundle>();
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        public void SetBundles(Spread<RestBundle> Bundles)
        {
            foreach (RestBundle bundle in Bundles) {
                if (bundle != null) 
                { 
                    this._restBundles.Add(bundle);
                }
            }
            
        }


        public void Update()
        {
            if (this.client != null && _restBundles.Count() > 0)
            {
                foreach (var rest in _restBundles) 
                {
                    if(rest!=null)
                    {
                        rest.Update(this.client);
                    }
                        
                }
            }
            
        }

        public void Split(out Spread<string> ContentTypes, out Spread<RestBundle> Bundles )
        {
            ContentTypes = this.client.AcceptedContentTypes.ToSpread();
            Bundles = this._restBundles.Where(x=>x.Execute == true).ToSpread();
        }
    }
}
