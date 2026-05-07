FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/ClientPlatform.Web.Host/ClientPlatform.Web.Host.csproj", "src/ClientPlatform.Web.Host/"]
COPY ["src/ClientPlatform.Web.Core/ClientPlatform.Web.Core.csproj", "src/ClientPlatform.Web.Core/"]
COPY ["src/ClientPlatform.Application/ClientPlatform.Application.csproj", "src/ClientPlatform.Application/"]
COPY ["src/ClientPlatform.Core/ClientPlatform.Core.csproj", "src/ClientPlatform.Core/"]
COPY ["src/ClientPlatform.EntityFrameworkCore/ClientPlatform.EntityFrameworkCore.csproj", "src/ClientPlatform.EntityFrameworkCore/"]
WORKDIR "/src/src/ClientPlatform.Web.Host"
RUN dotnet restore

WORKDIR /src
COPY ["src/ClientPlatform.Web.Host", "src/ClientPlatform.Web.Host"]
COPY ["src/ClientPlatform.Web.Core", "src/ClientPlatform.Web.Core"]
COPY ["src/ClientPlatform.Application", "src/ClientPlatform.Application"]
COPY ["src/ClientPlatform.Core", "src/ClientPlatform.Core"]
COPY ["src/ClientPlatform.EntityFrameworkCore", "src/ClientPlatform.EntityFrameworkCore"]
WORKDIR "/src/src/ClientPlatform.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "ClientPlatform.Web.Host.dll"]
