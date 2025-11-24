using System;

namespace Microsoft.Health.Fhir.Liquid.Converter.Api.Helper
{
    public class Hl7v2ParsingException : Exception
    {
        public int? LineNumber { get; }

        public Hl7v2ParsingException(string message)
            : base(message)
        {
        }

        public Hl7v2ParsingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public Hl7v2ParsingException(string message, int lineNumber)
            : base(message)
        {
            LineNumber = lineNumber;
        }
    }

}
