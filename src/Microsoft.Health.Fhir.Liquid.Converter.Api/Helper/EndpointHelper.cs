using Microsoft.Health.Fhir.Liquid.Converter.Api.Handler;
using System.Text.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Api.Helper
{
    public static class EndpointHelper
    {
        public static async Task<IResult> ExecuteSafe(Func<Task<IResult>> func)
        {
            try
            {
                return await func();
            }
            catch (TemplateNotFoundException ex)
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "Template not found",
                    Details = ex.Message
                });
            }
            catch (Hl7v2ParsingException ex)
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "Invalid HL7 format",
                    Details = ex.Message
                });
            }
            catch (JsonException ex)
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Error = "Invalid JSON input",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(title: "Internal Error", detail: ex.Message);
            }
        }
    }
}
