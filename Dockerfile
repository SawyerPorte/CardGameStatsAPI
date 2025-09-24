# 1. Use the .NET 7 SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy everything into the container
COPY . ./

# Restore dependencies
RUN dotnet restore

# Build the project (adjust path if your .csproj is in a subfolder)
RUN dotnet build TDDBackendStats.csproj -c Release -o /app/build

# Publish the app for deployment
RUN dotnet publish TDDBackendStats.csproj -c Release -o /app/publish

# 2. Use the smaller runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

# Copy the published files from the build stage
COPY --from=build /app/publish .

# Tell ASP.NET Core to listen on the Railway port
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE $PORT

# Start the app
ENTRYPOINT ["dotnet", "TDDBackendStats.dll"]