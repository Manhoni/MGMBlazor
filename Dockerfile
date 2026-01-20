
# ===============================
# STAGE 1 - Build
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia csproj e restaura dependências
COPY MGMBlazor.web.csproj ./
RUN dotnet restore

# Copia o restante do código
COPY . ./

# Publica a aplicação
RUN dotnet publish MGMBlazor.web.csproj -c Release -o /app/publish

# ===============================
# STAGE 2 - Runtime
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MGMBlazor.web.dll"]
