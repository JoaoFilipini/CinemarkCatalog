FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/MovieCatalog/MovieCatalog.API/MovieCatalog.API.csproj", "src/Services/MovieCatalog/MovieCatalog.API/"]
COPY ["src/Services/MovieCatalog/MovieCatalog.Application/MovieCatalog.Application.csproj", "src/Services/MovieCatalog/MovieCatalog.Application/"]
COPY ["src/Services/MovieCatalog/MovieCatalog.Domain/MovieCatalog.Domain.csproj", "src/Services/MovieCatalog/MovieCatalog.Domain/"]
COPY ["src/Services/MovieCatalog/MovieCatalog.Infra/MovieCatalog.Infra.csproj", "src/Services/MovieCatalog/MovieCatalog.Infra/"]
RUN dotnet restore "src/Services/MovieCatalog/MovieCatalog.API/MovieCatalog.API.csproj"
COPY . .
WORKDIR "/src/src/Services/MovieCatalog/MovieCatalog.API"
RUN dotnet build "MovieCatalog.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MovieCatalog.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MovieCatalog.API.dll"]
