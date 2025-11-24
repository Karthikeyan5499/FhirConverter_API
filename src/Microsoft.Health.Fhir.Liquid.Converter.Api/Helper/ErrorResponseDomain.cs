namespace Microsoft.Health.Fhir.Liquid.Converter.Api.Handler
{
    public class ErrorResponse
    {
        public string Error { get; set; }
        public string Details { get; set; }
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }

}
