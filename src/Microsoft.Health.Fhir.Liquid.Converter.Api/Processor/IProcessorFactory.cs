using Microsoft.Health.Fhir.Liquid.Converter.Processors;

namespace Microsoft.Health.Fhir.Liquid.Converter.Api.Processor
{
    public interface IProcessorFactory
    {
        BaseProcessor GetProcessor(string type);
    }
}