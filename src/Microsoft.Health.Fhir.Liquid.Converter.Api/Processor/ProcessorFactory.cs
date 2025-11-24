using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;

namespace Microsoft.Health.Fhir.Liquid.Converter.Api.Processor
{
    public class ProcessorFactory : IProcessorFactory
    {
        private readonly Hl7v2Processor _hl7;
        private readonly JsonProcessor _json;
        private readonly CcdaProcessor _ccda;

        public ProcessorFactory(
            Hl7v2Processor hl7,
            JsonProcessor json,
            CcdaProcessor ccda)
        {
            _hl7 = hl7;
            _json = json;
            _ccda = ccda;
        }

        public BaseProcessor GetProcessor(string type)
        {
            return type.ToLower() switch
            {
                ProcessorTypes.Hl7v2 => _hl7,
                ProcessorTypes.Json => _json,
                ProcessorTypes.Ccda => _ccda,
                _ => throw new InvalidOperationException($"Processor type '{type}' is not supported")
            };
        }
    }   
}
