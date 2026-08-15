FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/src/Padel.Domain/Padel.Domain.csproj src/Padel.Domain/
COPY backend/src/Padel.Application/Padel.Application.csproj src/Padel.Application/
COPY backend/src/Padel.Infrastructure/Padel.Infrastructure.csproj src/Padel.Infrastructure/
COPY backend/src/Padel.Api/Padel.Api.csproj src/Padel.Api/
RUN dotnet restore src/Padel.Api/Padel.Api.csproj

COPY backend/src/ src/
RUN dotnet publish src/Padel.Api/Padel.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Render injects PORT; Kestrel binds to it via ASPNETCORE_URLS below.
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app ./

EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet Padel.Api.dll"]
