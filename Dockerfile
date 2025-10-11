# Этап 1: Сборка с использованием полного .NET SDK (.NET 9)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем .csproj файлы для обоих проектов для эффективного кэширования слоев
COPY ["FunAndChecks/FunAndChecks.csproj", "FunAndChecks/"]
COPY ["AdminUI/AdminUI.csproj", "AdminUI/"]

# Восстанавливаем NuGet-пакеты
RUN dotnet restore "FunAndChecks/FunAndChecks.csproj"

# Копируем ИСХОДНЫЙ КОД только для нужных проектов
COPY ["FunAndChecks/", "FunAndChecks/"]
COPY ["AdminUI/", "AdminUI/"]

# Публикуем основной API проект
RUN dotnet publish "FunAndChecks/FunAndChecks.csproj" -c Release -o /app/publish /p:UseAppHost=false


# Этап 2: Создание финального, легковесного образа
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

USER root
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER app

ENTRYPOINT ["dotnet", "FunAndChecks.dll"]