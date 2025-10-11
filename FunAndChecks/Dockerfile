FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
USER $APP_UID
EXPOSE 8080
EXPOSE 8081

COPY ["FunAndChecks/FunAndChecks.csproj", "FunAndChecks/"]
COPY ["FunAndChecks.AdminUI/FunAndChecks.AdminUI.csproj", "FunAndChecks.AdminUI/"]

RUN dotnet restore "FunAndChecks/FunAndChecks.csproj"

COPY ["FunAndChecks/", "FunAndChecks/"]
COPY ["FunAndChecks.AdminUI/", "FunAndChecks.AdminUI/"]

# Публикуем
WORKDIR /src/FunAndChecks
RUN dotnet publish "FunAndChecks.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап 2: Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

RUN adduser --system --group --no-create-home app

USER root

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* 

USER app

# HEALTHCHECK --interval=10s --timeout=5s --start-period=20s --retries=5 CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "FunAndChecks.dll"]