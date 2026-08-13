FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SmartAPIForge.slnx .
COPY src/SmartAPIForge.Domain/SmartAPIForge.Domain.csproj src/SmartAPIForge.Domain/
COPY src/SmartAPIForge.Application/SmartAPIForge.Application.csproj src/SmartAPIForge.Application/
COPY src/SmartAPIForge.Infrastructure/SmartAPIForge.Infrastructure.csproj src/SmartAPIForge.Infrastructure/
COPY src/SmartAPIForge.Api/SmartAPIForge.Api.csproj src/SmartAPIForge.Api/
RUN dotnet restore src/SmartAPIForge.Api/SmartAPIForge.Api.csproj

COPY src/ src/
RUN dotnet publish src/SmartAPIForge.Api/SmartAPIForge.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SmartAPIForge.Api.dll"]
