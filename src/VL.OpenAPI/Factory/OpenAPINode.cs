using CommunityToolkit.HighPerformance;
using Microsoft.OpenApi;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using VL.Core;

namespace VL.OpenAPI
{
    sealed class OpenAPINode : FactoryBasedVLNode, IVLNode
    {
        readonly NodeDescription description;
        readonly Pin resultPin;
        
        readonly Pin runPin;

        VL.OpenAPI.RestBundle bundle;

        private string authParameterName;
        private Dictionary<string, object> parameters;
        // This is where we'll run the queries to the Directus instance
        
        public OpenAPINode(NodeDescription description, NodeContext nodeContext) : base(nodeContext)
        {
            this.description = description;

            Inputs = description.Inputs.Select(p => new Pin() { Name = p.Name, OriginalName = ((PinDescription)p).OriginalName, Type = p.Type, Value = p.DefaultValue }).ToArray();
            Outputs = description.Outputs.Select(p => new Pin() { Name = p.Name, OriginalName = ((PinDescription)p).OriginalName, Type = p.Type, Value = p.DefaultValue }).ToArray();

            
            resultPin = Outputs.FirstOrDefault(o => o.Name == "Result");
            runPin = Inputs.LastOrDefault();

            parameters = new Dictionary<string, object>();

            foreach(var p in description.Parameters)
            {
                if (p != null)
                    parameters.TryAdd(p.Name, null);
            }
            // Create RestClient & RestRequest
            //client = new RestClient(description.FEndpoint + description.FPath);
           
            bundle = new RestBundle(this.description.FItemPath.Key, description.FOperation.Key.ToString());
           
            
            //bundle.GetRequest().Parameters.AddParameter(description.Inputs.Select(x=>x.Name));
            //response = new RestResponse();
            // Look for authentication stuff
           /* try
            {
                //authParameterName = description.FSecuritySchemes.FirstOrDefault(x => x.Value.In == ParameterLocation.Query).Value.Name;
                authParameterName = "";
                request.AddOrUpdateParameter(authParameterName, description.FAPIKey);
                Console.WriteLine("Added auth parameter " + authParameterName);
            }
            catch (Exception ex)
            {
                authParameterName = "";
                Console.WriteLine("No query-based authentication found");
            }*/
        }

        public IVLNodeDescription NodeDescription => description;

        public Pin[] Inputs { get; }
        public Pin[] Outputs { get; }

        public void Update()
        {
            if (runPin is null || !(bool)runPin.Value)
            {
                this.bundle.Execute = false;
                // Clear all params except auth!
                // Is it better to do that or just create a new request?
                foreach (var param in bundle.GetRequest().Parameters.Where(x => x.Name != authParameterName))
                {
                    bundle.GetRequest().Parameters.RemoveParameter(param);
                }
                
                return;
            }
            
            this.bundle.Execute = true;
            
            

            // Look for pins that actually have a value and add them as params
            foreach(var input in Inputs.Cast<Pin>().SkipLast(1))
            {
                // That looks a bit convoluted
                if (input.Type == typeof(IEnumerable<string>) && ((int)typeof(ICollection).GetProperty("Count").GetValue(input.Value, null)) > 0)
                {
                    bundle.UpdateParameter(input.OriginalName, string.Join(",", (IEnumerable<string>)input.Value));
                }
                else if (input.Type == typeof(int))
                {
                    Console.WriteLine($"Update {input.Name}  --> {input.OriginalName}");
                    bundle.UpdateParameter(input.OriginalName, input.Value.ToString());
                }
                else if (input.Type == typeof(string) && !(string.IsNullOrEmpty(input.Value as string)))
                {
                    Console.WriteLine($"Update {input.Name}  --> {input.OriginalName}");
                    bundle.UpdateParameter(input.OriginalName, (string)input.Value);

                }
                else if (input.Type == typeof(bool))
                {
                    // TBD
                }
                else
                {
                    Console.WriteLine($"Update {input.Name}  --> {input.OriginalName}");
                    bundle.UpdateParameter(input.OriginalName, (string)input.Value);
                }
            }

            //var response = client.Execute(request);

            resultPin.Value = this.bundle;
            
            //resultPin.Value = this;
        }

        public void Dispose()
        {
            Console.WriteLine("Byyyye");
        }


        public override string ToString()
        {
            return base.ToString();
        }

        

        IVLPin[] IVLNode.Inputs => Inputs;
        IVLPin[] IVLNode.Outputs => Outputs;
    }
}