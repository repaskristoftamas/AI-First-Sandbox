FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/backend/Bookstore.SharedKernel/Bookstore.SharedKernel.csproj src/backend/Bookstore.SharedKernel/
COPY src/backend/Bookstore.Domain/Bookstore.Domain.csproj src/backend/Bookstore.Domain/
COPY src/backend/Bookstore.Application/Bookstore.Application.csproj src/backend/Bookstore.Application/
COPY src/backend/Bookstore.Infrastructure/Bookstore.Infrastructure.csproj src/backend/Bookstore.Infrastructure/
COPY src/backend/Bookstore.WebApi/Bookstore.WebApi.csproj src/backend/Bookstore.WebApi/
RUN dotnet restore src/backend/Bookstore.WebApi/Bookstore.WebApi.csproj

COPY src/backend/ src/backend/
RUN dotnet publish src/backend/Bookstore.WebApi/Bookstore.WebApi.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bookstore.WebApi.dll"]
