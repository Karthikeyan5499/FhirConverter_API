using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Liquid.Converter.Models
{
    public static class TemplateGlobals
    {
        public static string SourceSystemName { get; set; } = "DEFAULT-SYSTEM";
        public static string SourceSystemPath { get; set; } = "DEFAULT-PATH";
        public static string HL7MessageCode { get; set; } = "DEFAULT-MSG-CODE";
        public static string ExtensionIdentifierValue { get; set; } = Guid.NewGuid().ToString().ToUpper();
        public static string Hl7Base64 { get; set; }
    }
}
