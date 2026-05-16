FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy csproj files first for layer caching
COPY ["src/FileHubOrg/FileHubOrg.Web/FileHubOrg.Web.csproj", "src/FileHubOrg/FileHubOrg.Web/"]
COPY ["src/FileHubOrg/FileHubOrg.Application/FileHubOrg.Application.csproj", "src/FileHubOrg/FileHubOrg.Application/"]
COPY ["src/FileHubOrg/FileHubOrg.Infrastructure/FileHubOrg.Infrastructure.csproj", "src/FileHubOrg/FileHubOrg.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/FileHubOrg/FileHubOrg.Web/FileHubOrg.Web.csproj"

# Copy all source code
COPY src/FileHubOrg/ src/FileHubOrg/

# Publish app
RUN dotnet publish "src/FileHubOrg/FileHubOrg.Web/FileHubOrg.Web.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FileHubOrg.Web.dll"]