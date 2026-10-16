# Build from the Kelpford folder so Directory.Build.props is in the context:
#   docker build -f src/Kelpford.Web/Dockerfile -t kelpford-web .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION=1.0.0
WORKDIR /src
COPY Directory.Build.props ./
COPY src/Kelpford.Web/Kelpford.Web.csproj src/Kelpford.Web/
RUN dotnet restore src/Kelpford.Web/Kelpford.Web.csproj
COPY src/Kelpford.Web/ src/Kelpford.Web/
RUN dotnet publish src/Kelpford.Web/Kelpford.Web.csproj -c Release -o /app/publish --no-restore /p:Version=$VERSION /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Kelpford.Web.dll"]
