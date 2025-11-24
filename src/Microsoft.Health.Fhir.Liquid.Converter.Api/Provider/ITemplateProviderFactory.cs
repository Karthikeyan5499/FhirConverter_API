namespace Microsoft.Health.Fhir.Liquid.Converter.Api.Provider
{
    public interface ITemplateProviderFactory
    {
        ITemplateProvider GetProvider(string processorType);
    }
}
