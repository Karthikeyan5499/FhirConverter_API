using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Health.Fhir.Liquid.Converter.Helper
{
    public class Helper
    {
        public static string GetHl7TemplateType(string hl7Message)
        {
            if (string.IsNullOrWhiteSpace(hl7Message))
            {
                return null;
            }

            // Get the first line (MSH segment)
            var firstLine = hl7Message.Split('\n', '\r')
                                      .FirstOrDefault(l => l.StartsWith("MSH"));

            if (string.IsNullOrWhiteSpace(firstLine))
            {
                return null;
            }

            // HL7 fields are separated by "|"
            var fields = firstLine.Split('|');

            // MSH-9 = MessageType (e.g., ADT^A01)
            if (fields.Length > 8)
            {
                var result = fields[8].Replace("^", "_");
                return result;
            }

            return null;
        }

    }
}
