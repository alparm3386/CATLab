# syntax=docker/dockerfile:1

# Use the .NET SDK as a base image for building the project
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env

# Set the ASPNETCORE_ENVIRONMENT environment variable to Development
ENV ASPNETCORE_ENVIRONMENT=Development

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
COPY --from=build-env /app/online-editor-ui/build/ /app/online-editor-ui/build/


EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "CAT-OnlineEditor.dll"]
