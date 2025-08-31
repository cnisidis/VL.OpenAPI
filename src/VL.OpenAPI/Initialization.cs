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
        const string apisSubDir = "apis";

        public override void Configure(AppHost appHost)
        {
            appHost.RegisterNodeFactory("VL.OpenAPI-Factory", (directory, nodeFactory) =>
            {
                var invalidated = NodeBuilding.WatchDir(directory)
                .Where(e => (e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Deleted || e.ChangeType == WatcherChangeTypes.Renamed || e.ChangeType == WatcherChangeTypes.All) && e.Name == apisSubDir);

                var builder = ImmutableArray.CreateBuilder<IVLNodeDescription>();
                var apiDir = Path.Combine(directory, apisSubDir);

                Console.WriteLine(apiDir);
                Console.WriteLine("Factory initialized");

                if (Directory.Exists(apiDir)) {

                    Console.WriteLine("Directory:", directory);
                    Console.WriteLine("APIs Directory: {0}", apiDir);
                    Console.WriteLine("APIs Directory: {0}", apisSubDir);
                    string[] restAPIDescriptions = Directory.GetFiles(apiDir, "*.*").Where(x=>Path.GetExtension(x)==".yaml" || Path.GetExtension(x)==".json").ToArray();
                    if(restAPIDescriptions.Length >0 )
                    {
                        foreach ( string apiDescription in restAPIDescriptions ) { Console.WriteLine(Path.GetExtension(apiDescription)); }
                        
                    }
                }

                return new(builder.ToImmutable(), invalidated);
            });
        }

        static IVLNodeDescriptionFactory openAPIFactory = new OpenAPINodeFactory();
    }    
}