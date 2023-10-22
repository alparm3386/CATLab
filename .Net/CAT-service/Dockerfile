# syntax=docker/dockerfile:1

# Use the .NET SDK as a base image for building the project
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env
WORKDIR /app

# Copy the .csproj file and restore any dependencies (via dotnet restore)
COPY *.csproj .
RUN dotnet restore

# Publish the application to the /publish directory in Release configuration
COPY . .

# Set the DOTNET_ENVIRONMENT environment variable to Development
ENV DOTNET_ENVIRONMENT Development

RUN dotnet publish -c Debug -o /publish

# Use the .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime
WORKDIR /app
COPY --from=build-env /publish .

EXPOSE 5001
ENTRYPOINT ["dotnet", "CAT-service.dll"]
#ENTRYPOINT ["tail", "-f", "/dev/null"]
#ENTRYPOINT ["dotnet", "CAT-service.dll", "--server", "nmi"]
