# ----------------------
# Build Stage
# ----------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the solution file
COPY 12decnet.sln ./

# Copy API project files
COPY api/*.csproj ./api/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Publish the API
RUN dotnet publish api -c Release -o /app/publish

# ----------------------
# Runtime Stage
# ----------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy published API from build stage
COPY --from=build /app/publish .

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Run the API
ENTRYPOINT ["dotnet", "api.dll"]
