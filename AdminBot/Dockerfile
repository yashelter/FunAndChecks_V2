FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем .csproj файлы для AdminBot и его зависимости FunAndChecks
COPY ["FunAndChecks/FunAndChecks.csproj", "FunAndChecks/"]
COPY ["AdminBot/AdminBot.csproj", "AdminBot/"]
RUN dotnet restore "AdminBot/AdminBot.csproj"

# Копируем исходный код ТОЛЬКО нужных проектов
COPY FunAndChecks/ ./FunAndChecks/
COPY AdminBot/ ./AdminBot/
COPY .config/ ./.config/

RUN rm FunAndChecks/appsettings*.json
RUN dotnet tool restore

# Публикуем
WORKDIR /src/AdminBot
RUN dotnet publish "AdminBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап 2: Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AdminBot.dll"]