FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/Notifications/NotificationConsumer.Service/NotificationConsumer.Service.csproj", "src/Services/Notifications/NotificationConsumer.Service/"]
RUN dotnet restore "src/Services/Notifications/NotificationConsumer.Service/NotificationConsumer.Service.csproj"
COPY . .
WORKDIR "/src/src/Services/Notifications/NotificationConsumer.Service"
RUN dotnet build "NotificationConsumer.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationConsumer.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationConsumer.Service.dll"]
