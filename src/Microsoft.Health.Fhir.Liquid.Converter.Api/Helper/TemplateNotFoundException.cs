using System;

namespace Microsoft.Health.Fhir.Liquid.Converter.Api.Helper
{
    public class TemplateNotFoundException : Exception
    {
        public string TemplateName { get; }

        public TemplateNotFoundException(string templateName)
            : base($"Template '{templateName}' was not found.")
        {
            TemplateName = templateName;
        }

        public TemplateNotFoundException(string templateName, string message)
            : base(message)
        {
            TemplateName = templateName;
        }

        public TemplateNotFoundException(string templateName, string message, Exception innerException)
            : base(message, innerException)
        {
            TemplateName = templateName;
        }
    }

}
