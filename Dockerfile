﻿# Use the .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ENV ASPNETCORE_ENVIRONMENT=Development 

WORKDIR /app

# Copy the solution file and restore dependencies
COPY MediPort.sln .
COPY MediPort.Api ./MediPort.Api/
COPY MediPort.RestApi ./MediPort.RestApi/

RUN dotnet restore "MediPort.Api/MediPort.Api.csproj"
RUN dotnet restore "MediPort.RestApi/MediPort.RestApi.csproj"

# Publish the application

RUN dotnet publish MediPort.RestApi/MediPort.RestApi.csproj -c Release -o /app/MediPort.RestApi/out

# Use a smaller runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

EXPOSE 8080

# Copy the published output from the build stage to the final stage
COPY --from=build /app/MediPort.RestApi/out ./

# Copy Certificate
RUN mkdir -p /app/certs
COPY certs/MediPort.RestApi.pfx /app/certs/MediPort.RestApi.pfx

RUN update-ca-certificates

# Set the entry point for the container
ENTRYPOINT ["dotnet", "MediPort.RestApi.dll", "--environment=Development"]