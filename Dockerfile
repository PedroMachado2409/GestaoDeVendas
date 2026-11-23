FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props ./
COPY GestaoPedidos/GestaoPedidos.csproj GestaoPedidos/
RUN dotnet restore GestaoPedidos/GestaoPedidos.csproj

COPY GestaoPedidos/ GestaoPedidos/
RUN dotnet publish GestaoPedidos/GestaoPedidos.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish ./

USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "GestaoPedidos.dll"]
