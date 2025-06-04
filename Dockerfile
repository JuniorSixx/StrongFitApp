# Etapa base para rodar o app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# 🔐 Variáveis do OAuth2 do Google (Render lê isso do painel ou .env local)
ENV Authentication__Google__ClientId=${Authentication__Google__ClientId}
ENV Authentication__Google__ClientSecret=${Authentication__Google__ClientSecret}

# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "StrongFitApp.csproj"
RUN dotnet publish "StrongFitApp.csproj" -c Release -o /app/publish

# Etapa final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StrongFitApp.dll"]
