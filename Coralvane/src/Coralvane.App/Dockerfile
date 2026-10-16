# Build from the Coralvane folder so Directory.Build.props is in the context:
#   docker build -f src/Coralvane.App/Dockerfile -t coralvane-app .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION=1.0.0
WORKDIR /src
COPY Directory.Build.props ./
COPY src/Coralvane.App/Coralvane.App.csproj src/Coralvane.App/
RUN dotnet restore src/Coralvane.App/Coralvane.App.csproj
COPY src/Coralvane.App/ src/Coralvane.App/
RUN dotnet publish src/Coralvane.App/Coralvane.App.csproj -c Release -o /app/publish --no-restore /p:Version=$VERSION /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Coralvane.App.dll"]
