# Use .NET 7 SDK
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy everything into the container
COPY . ./

# Restore dependencies
RUN dotnet restore TDDBackendStats/TDDBackendStats.csproj

# Build the project
RUN dotnet build TDDBackendStats/TDDBackendStats.csproj -c Release -o /app/build

# Publish the project
RUN dotnet publish TDDBackendStats/TDDBackendStats.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE $PORT

ENTRYPOINT ["dotnet", "TDDBackendStats.dll"]