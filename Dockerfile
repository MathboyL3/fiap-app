# syntax=docker/dockerfile:1.7
# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Restore (camada cacheável)
COPY src/Oficina.Domain/*.csproj         src/Oficina.Domain/
COPY src/Oficina.Application/*.csproj    src/Oficina.Application/
COPY src/Oficina.Infrastructure/*.csproj src/Oficina.Infrastructure/
COPY src/Oficina.Api/*.csproj            src/Oficina.Api/
RUN dotnet restore src/Oficina.Api/Oficina.Api.csproj

# Build + publish
COPY src/ src/
RUN dotnet publish src/Oficina.Api/Oficina.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Usuário não-root (princípio do menor privilégio - vide DevSeguro Aula 1)
# A imagem base do .NET 10 já cria o usuário/grupo "app" — só criamos se não existir.
RUN (getent group app  > /dev/null || groupadd --system --gid 1001 app) && \
    (id -u app         > /dev/null 2>&1 || useradd --system --uid 1001 --gid app --shell /sbin/nologin app)

COPY --from=build --chown=app:app /app/publish .

USER app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=false

ENTRYPOINT ["dotnet", "Oficina.Api.dll"]
