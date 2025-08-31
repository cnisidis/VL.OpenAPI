using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Reactive.Linq;
using VL.Core;
using VL.Core.CompilerServices;

// Tell VL where to find the initializer
[assembly: AssemblyInitializer(typeof(VL.OpenAPI.Initialization))]

namespace VL.OpenAPI
{
    public class Initialization : AssemblyInitializer<Initialization>
    {
        const string apisSubDir = "openApi";

        public override void Configure(AppHost appHost)
        {
            appHost.RegisterNodeFactory("VL.OpenAPI-Factory", (directory, nodeFactory) =>
            {
                

                var builder = ImmutableArray.CreateBuilder<IVLNodeDescription>();
                var apiDir = Path.Combine(directory, apisSubDir);
                var invalidated = NodeBuilding.WatchDir(directory);



                Console.WriteLine(apiDir);
                Console.WriteLine("Factory initialized");

                if (Directory.Exists(apiDir)) {
                    invalidated = NodeBuilding.WatchDir(apiDir)
                .Where(e => (e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Deleted || e.ChangeType == WatcherChangeTypes.Renamed || e.ChangeType == WatcherChangeTypes.All) && e.Name == apisSubDir);

                    //Console.WriteLine("Directory:", directory);
                    //Console.WriteLine("APIs Directory: {0}", apiDir);
                    //Console.WriteLine("APIs Directory: {0}", apisSubDir);
                    string[] restAPIDescriptions = Directory.GetFiles(apiDir, "*.*").Where(x=>Path.GetExtension(x)==".yaml" || Path.GetExtension(x)==".json").ToArray();
                    if(restAPIDescriptions.Length >0 )
                    {
                        
                        
                        foreach ( string apiDescriptionFile in restAPIDescriptions ) { 
                            var ext = Path.GetExtension(apiDescriptionFile).Substring(1);
                            var fname = Path.GetFileName(apiDescriptionFile).Split('.').FirstOrDefault();
                            if(ext == "yaml" ||  ext =="json")
                            {
                                var Parser = new Parser();
                                Console.WriteLine(fname);
                                Parser.FromFile(apiDescriptionFile);
                                if(Parser.openApiDiagnostic.Errors.Count >0)
                                {
                                    foreach(var err in  Parser.openApiDiagnostic.Errors)
                                    {
                                        Console.WriteLine(err.Message);
                                    }
                                }
                                
                                foreach (var path in Parser.openApiDoc.Paths)
                                {
                                    
                                    foreach (var operation in path.Value.Operations)
                                    {
                                        if (operation.Key != null && operation.Value != null)
                                        {
                                            
                                            builder.Add(new NodeDescription(nodeFactory, Utils.ToPascalCase(fname) + "." + ext.ToUpper(), operation.Value, path, operation.Key));
                                        }
                                            
                                    }
                                }
                            }
                                
                            
                            Console.WriteLine(apiDescriptionFile); 

                        }
                        
                    }
                }

                return new(builder.ToImmutable(), invalidated);
            });
        }

        //static IVLNodeDescriptionFactory openAPIFactory = new OpenAPINodeFactory();
    }    
}