# Build context is the `server/` directory (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/GoldFieldsHR.Domain/GoldFieldsHR.Domain.csproj src/GoldFieldsHR.Domain/
COPY src/GoldFieldsHR.Application/GoldFieldsHR.Application.csproj src/GoldFieldsHR.Application/
COPY src/GoldFieldsHR.Infrastructure/GoldFieldsHR.Infrastructure.csproj src/GoldFieldsHR.Infrastructure/
COPY src/GoldFieldsHR.Api/GoldFieldsHR.Api.csproj src/GoldFieldsHR.Api/
RUN dotnet restore src/GoldFieldsHR.Api/GoldFieldsHR.Api.csproj

COPY src/ src/
RUN dotnet publish src/GoldFieldsHR.Api/GoldFieldsHR.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "GoldFieldsHR.Api.dll"]
