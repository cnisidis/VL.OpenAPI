using System.Reactive.Linq;
using VL.Core;

using Microsoft.OpenApi;
using VL.Core.Diagnostics;
using RestSharp;


namespace VL.OpenAPI
{
    sealed class NodeDescription : IVLNodeDescription, IInfo
    {
        // Fields
        bool FInitialized;
        bool FError;

        string? FSummary;
        string FCategory;

        private string authParameterName = "";
        private HttpMethod FMethod = new HttpMethod("GET");
        // Inputs and outputs
        List<PinDescription> inputs = new List<PinDescription>();
        List<PinDescription> outputs = new List<PinDescription>();

        public KeyValuePair<HttpMethod, OpenApiOperation> FOperation;
        public KeyValuePair<string, IOpenApiPathItem> FItemPath;

        public List<IOpenApiParameter> Parameters;
        
        public NodeDescription(IVLNodeDescriptionFactory factory, string category, KeyValuePair<HttpMethod, OpenApiOperation> operation, KeyValuePair<string,IOpenApiPathItem> itemPath) 
        {
            Factory = factory;
            
            FOperation = operation;
            FItemPath = itemPath;

            Name = Utils.ToPascalCase(operation.Value.OperationId);
            
            FSummary = operation.Value.Description ?? "";
            FEndpoint = "";
            FAPIKey = "";
            FMethod = operation.Key;
            FCategory = category +"."+ operation.Key.ToString();
            
            
            inputs = new();
            outputs = new();
            FInitialized = false;
            Parameters = new();
            
            
        }

        void Init()
        {
            if (FInitialized)
                return;

            try
            {
                Type type = typeof(object);
                object dflt = "";
                string name = "";
                string desc = "";

                var pathItemParameters = FItemPath.Value.Parameters ?? new List<IOpenApiParameter>();
                var operationParameters = FOperation.Value.Parameters ?? new List<IOpenApiParameter>();

                Parameters = pathItemParameters.Union(operationParameters).ToList();
                // Retrieve parameters from the OpenAPI dump and create input pins
                if (FOperation.Value != null && Parameters.Count() > 0)
                {
                    
                    foreach (var parameter in Parameters)
                    {
                        if (parameter != null)
                        {
                            //    GetTypeDefaultAndDescription(parameter, ref type, ref dflt, ref desc);
                            inputs.Add(new PinDescription(parameter.Name, type, dflt, parameter.Description));
                        }

                    }
                }


                // Adds the trigger pin
                inputs.Add(new PinDescription("Execute", typeof(bool), false, "Sends a query as long as enabled"));

                // For now let's just get the raw JSON response from Directus. Create a single string output pin
                //outputs.Add(new PinDescription("Result", typeof(string),"", "The raw string response"));
                
                outputs.Add(new PinDescription("Result", typeof(RestBundle), null, "The Result Pin"));

                FInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        void GetTypeDefaultAndDescription(OpenApiParameter parameter, ref Type type, ref object dflt, ref string desc)
        {
            desc = parameter.Description;

            /*if(parameter.Schema.Type == "string")
            {
                type = typeof(string);
                dflt = "";
            }
            else if(parameter.Schema.Type == "boolean")
            {
                type = typeof(bool);
                dflt = false;
            }
            else if(parameter.Schema.Type == "array")
            {
                if(parameter.Schema.Items.Type == "string")
                {
                    type = typeof(IEnumerable<string>);
                    dflt = Enumerable.Repeat<string>("", 0).ToArray();
                }
            }*/
        }
        public IVLNodeDescriptionFactory Factory { get; }
        public bool Fragmented => false;

        public string FEndpoint;
        public string FPath;
        //public KeyValuePair<OperationType, OpenApiOperation> FOperation;
        public IDictionary<string, SecuritySchemeType> FSecuritySchemes;
        public string FAPIKey;
        
        public string Name { get; }
        public string Category => FCategory;
        
        public IReadOnlyList<IVLPinDescription> Inputs
        {
            get
            {
                Init();
                return inputs;
            }    
        }
        public IReadOnlyList<IVLPinDescription> Outputs
        {
            get
            {
                Init();
                return outputs;
            }
        }

        public IEnumerable<Core.Diagnostics.Message> Messages
        {
            get
            {
                if(FError) 
                    yield return new Message(MessageType.Warning, "Grrrr");
                else
                    yield break;
            }    
        }

        public string Summary => FSummary;
        public string Remarks => "";
        public IObservable<object> Invalidated => Observable.Empty<object>();
        public IVLNode CreateInstance(NodeContext context)
        {
            return new OpenAPINode(this, context);
        }
        public bool OpenEditor()
        {
            return true;
        }
    }
}