# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY ["mcp.csproj", "."]
RUN dotnet restore
COPY . .
# Публикуем как single-file, self-contained для linux-musl-x64
RUN dotnet publish -c Release \
    --self-contained true \
    -r linux-musl-x64 \
    -p:PublishSingleFile=true \
    -o /app/publish

# Финальный образ — используем минимальный базовый образ Alpine без .NET
FROM alpine:3.22
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine
WORKDIR /app
# Копируем только скомпилированный бинарник
COPY --from=build /app/publish/mcp .
# Даём права на выполнение
RUN chmod +x ./mcp

# Порт НЕ EXPOSE — MCP работает через stdio
# Точка входа — сам бинарник
ENTRYPOINT ["./mcp"]
