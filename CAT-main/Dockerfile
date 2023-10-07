# syntax=docker/dockerfile:1

# Use the .NET SDK as a base image for building the project
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env
WORKDIR /app

# Copy the .csproj file and restore any dependencies (via dotnet restore)
COPY *.csproj .
RUN dotnet restore

# Publish the application to the /publish directory in Release configuration
COPY . .

RUN dotnet publish -c Debug -o /publish

# Use the .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime
WORKDIR /app
COPY --from=build-env /publish .
COPY --from=build-env /app/WebUI/Backoffice/angular/dist/monitoring /app/WebUI/backoffice/angular/dist/monitoring
COPY --from=build-env /app/WebUI/Backoffice/angular/dist/monitoring-details /app/WebUI/backoffice/angular/dist/monitoring-details

EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "CAT-main.dll"]
