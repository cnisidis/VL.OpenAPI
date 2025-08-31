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

        private OpenApiOperation FOperation = new OpenApiOperation();
        private KeyValuePair<string, IOpenApiPathItem> FItemPath;


        public NodeDescription(IVLNodeDescriptionFactory factory, string category, OpenApiOperation operation, KeyValuePair<string,IOpenApiPathItem> itemPath, HttpMethod method) 
        {
            Factory = factory;
            Name = Utils.ToPascalCase(operation.OperationId);
            FSummary = this.FOperation.Description;
            FEndpoint = "";
            FAPIKey = "";
            FMethod = method;
            FCategory = category +"."+ method.Method.ToString();
            this.FOperation = operation;
            this.FItemPath = itemPath;
            inputs = new();
            outputs = new();
            FInitialized = false;
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
                var operationParameters = FOperation.Parameters ?? new List<IOpenApiParameter>();
                
                var allParameters = pathItemParameters.Union(operationParameters);
                // Retrieve parameters from the OpenAPI dump and create input pins
                if (FOperation != null && allParameters.Count() > 0)
                {
                    
                    foreach (var parameter in allParameters)
                    {
                        if (parameter != null)
                        {
                            //    GetTypeDefaultAndDescription(parameter, ref type, ref dflt, ref desc);
                            inputs.Add(new PinDescription(Utils.ToPascalCase(parameter.Name), type, dflt, parameter.Description));
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