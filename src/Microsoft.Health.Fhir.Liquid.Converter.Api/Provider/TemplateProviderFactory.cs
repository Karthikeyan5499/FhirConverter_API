using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Api.Provider
{
    public class TemplateProviderFactory : ITemplateProviderFactory
    {
        private readonly string _root;

        public TemplateProviderFactory(IHostEnvironment env)
        {
            // Move 2 dirs up
            var root = Directory.GetParent(env.ContentRootPath)!.FullName;
            root = Directory.GetParent(root)!.FullName;

            _root = Path.Combine(root, "data", "Templates");
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
