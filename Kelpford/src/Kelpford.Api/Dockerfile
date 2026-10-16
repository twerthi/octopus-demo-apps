# Build from the Kelpford folder so Directory.Build.props is in the context:
#   docker build -f src/Kelpford.Api/Dockerfile -t kelpford-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION=1.0.0
WORKDIR /src
COPY Directory.Build.props ./
COPY src/Kelpford.Api/Kelpford.Api.csproj src/Kelpford.Api/
RUN dotnet restore src/Kelpford.Api/Kelpford.Api.csproj
COPY src/Kelpford.Api/ src/Kelpford.Api/
RUN dotnet publish src/Kelpford.Api/Kelpford.Api.csproj -c Release -o /app/publish --no-restore /p:Version=$VERSION /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Kelpford.Api.dll"]
