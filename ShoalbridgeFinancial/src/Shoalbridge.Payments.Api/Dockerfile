# Build from the ShoalbridgeFinancial folder so Directory.Build.props is in the context:
#   docker build -f src/Shoalbridge.Payments.Api/Dockerfile -t shoalbridge-payments-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION=1.0.0
WORKDIR /src
COPY Directory.Build.props ./
COPY src/Shoalbridge.Payments.Api/Shoalbridge.Payments.Api.csproj src/Shoalbridge.Payments.Api/
RUN dotnet restore src/Shoalbridge.Payments.Api/Shoalbridge.Payments.Api.csproj
COPY src/Shoalbridge.Payments.Api/ src/Shoalbridge.Payments.Api/
RUN dotnet publish src/Shoalbridge.Payments.Api/Shoalbridge.Payments.Api.csproj -c Release -o /app/publish --no-restore /p:Version=$VERSION /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Shoalbridge.Payments.Api.dll"]
