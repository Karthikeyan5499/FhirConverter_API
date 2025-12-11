using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Api.Provider
{
    public class TemplateProviderFactory : ITemplateProviderFactory
    {
        private readonly string _root;

        public TemplateProviderFactory(IHostEnvironment env)
        {
            string basePath;

            if (env.IsDevelopment())
            {
                // Local debug – move 2 directories up
                var root = Directory.GetParent(env.ContentRootPath)!.FullName;
                root = Directory.GetParent(root)!.FullName;

                basePath = root;
            }
            else
            {
                // Non-development (Docker, Prod, Staging, etc.)
                basePath = env.ContentRootPath;
            }

            _root = Path.Combine(basePath, "data", "Templates");
        }

        public ITemplateProvider GetProvider(string processorType)
        {
            string path = processorType.ToLower() switch
            {
                ProcessorTypes.Hl7v2 => Path.Combine(_root, "Hl7v2"),
                ProcessorTypes.Json => Path.Combine(_root, "Json"),
                ProcessorTypes.Ccda => Path.Combine(_root, "Ccda"),
                _ => throw new Exception($"Unknown processor type: {processorType}")
            };

            return new TemplateProvider(path);
        }
    }

}
