FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Padel.Domain/Padel.Domain.csproj src/Padel.Domain/
COPY src/Padel.Application/Padel.Application.csproj src/Padel.Application/
COPY src/Padel.Infrastructure/Padel.Infrastructure.csproj src/Padel.Infrastructure/
COPY src/Padel.Api/Padel.Api.csproj src/Padel.Api/
RUN dotnet restore src/Padel.Api/Padel.Api.csproj

COPY src/ src/
RUN dotnet publish src/Padel.Api/Padel.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Render injects PORT; Kestrel binds to it via ASPNETCORE_URLS below.
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app ./

EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet Padel.Api.dll"]
