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

# --- New Relic .NET agent (APM) ---
# Instalado a partir do tarball oficial (evita a chave apt legada DSA1024).
# A ativacao e por variaveis de ambiente (CORECLR_*) + NEW_RELIC_LICENSE_KEY,
# injetadas em runtime via K8s Secret — nada sensivel fica na imagem.
RUN apt-get update && apt-get install -y --no-install-recommends wget ca-certificates \
    && mkdir -p /usr/local/newrelic-dotnet-agent \
    && wget -qO /tmp/nr.tar.gz "https://download.newrelic.com/dot_net_agent/latest_release/newrelic-dotnet-agent_amd64.tar.gz" \
    && tar xzf /tmp/nr.tar.gz -C /usr/local/newrelic-dotnet-agent --strip-components=1 \
    && rm /tmp/nr.tar.gz \
    && apt-get purge -y wget && apt-get autoremove -y && apt-get clean && rm -rf /var/lib/apt/lists/*

# Variaveis do profiler CLR (ativam a instrumentacao). LICENSE KEY vem do ambiente.
ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
    NEW_RELIC_APP_NAME="fiap-app"

# Usuário não-root (princípio do menor privilégio)
RUN (getent group app  > /dev/null || groupadd --system --gid 1001 app) && \
    (id -u app         > /dev/null 2>&1 || useradd --system --uid 1001 --gid app --shell /sbin/nologin app) && \
    mkdir -p /usr/local/newrelic-dotnet-agent/logs && chown -R app:app /usr/local/newrelic-dotnet-agent

COPY --from=build --chown=app:app /app/publish .

USER app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=false

ENTRYPOINT ["dotnet", "Oficina.Api.dll"]
