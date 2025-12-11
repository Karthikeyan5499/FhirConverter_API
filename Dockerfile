FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY ./publish/ .

# Expose multiple ports
EXPOSE 80
EXPOSE 8080

# Let ASP.NET Core listen on both ports
ENV ASPNETCORE_URLS=http://+:80;http://+:8080

ENTRYPOINT ["dotnet", "Microsoft.Health.Fhir.Liquid.Converter.Api.dll"]