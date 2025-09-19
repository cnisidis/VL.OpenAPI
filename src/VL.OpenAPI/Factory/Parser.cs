using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.YamlReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VL.Core;

namespace VL.OpenAPI
{
    public class Parser
    {
        private string _filePath;
        private string _fileName;
        private string _fileExtension;

        private bool _hasTags;
        Microsoft.OpenApi.Reader.ReadResult result = null;
        IOpenApiReader reader = null;

        public Microsoft.OpenApi.OpenApiDocument openApiDoc = null;
        public OpenApiDiagnostic openApiDiagnostic = null;
        
        public void FromFile(string FilePath)
        {
            if (!File.Exists(FilePath)) return;
            var ext = Path.GetExtension(FilePath);
           
            
            if (ext ==".json")
                reader= new Microsoft.OpenApi.Reader.OpenApiJsonReader();
            else if(ext == ".yaml")
                reader = new Microsoft.OpenApi.YamlReader.OpenApiYamlReader();

            var result = new Microsoft.OpenApi.Reader.ReadResult();
            try
            {
                using (var streamReader = new StreamReader(FilePath))
                {
                    var bytes = new byte[streamReader.BaseStream.Length];
                    streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    streamReader.BaseStream.Read(bytes);
                    using (var memory = new MemoryStream(bytes))
                    {
                        this.result = reader.Read(memory, new Uri(FilePath), new Microsoft.OpenApi.Reader.OpenApiReaderSettings());
                        this.result.Deconstruct(out openApiDoc, out openApiDiagnostic);
                        if (openApiDoc != null && openApiDiagnostic.Errors.Count == 0)
                        {
                            if (openApiDoc.Tags.Count > 0) { _hasTags = true; }
                        }
                        else
                        {
                            Console.WriteLine("Error");
                            return;
                        }
                    }
                    streamReader.Close();

                }
            }
            catch (Exception ex) 
            { 
            
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            
        }

        
       
    }
}
