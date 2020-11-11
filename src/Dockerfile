FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src

COPY ["CrashBox.Models/CrashBox.Models.csproj", "CrashBox.Models/"]
RUN dotnet restore "CrashBox.Models/CrashBox.Models.csproj"
COPY CrashBox.Models ./CrashBox.Models
RUN dotnet build "CrashBox.Models/CrashBox.Models.csproj" -c Release -o /app/build

COPY ["CrashBox.Cosmos/CrashBox.Cosmos.csproj", "CrashBox.Cosmos/"]
RUN dotnet restore "CrashBox.Cosmos/CrashBox.Cosmos.csproj"
COPY CrashBox.Cosmos ./CrashBox.Cosmos
RUN dotnet build "CrashBox.Cosmos/CrashBox.Cosmos.csproj" -c Release -o /app/build

COPY ["CrashBox.WebApi/CrashBox.WebApi.csproj", "CrashBox.WebApi/"]
RUN dotnet restore "CrashBox.WebApi/CrashBox.WebApi.csproj"
COPY CrashBox.WebApi ./CrashBox.WebApi
RUN dotnet build "CrashBox.WebApi/CrashBox.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CrashBox.WebApi/CrashBox.WebApi.csproj" -c Release -o /app/publish

ENV COSMOS_DB_NAME="crashes"
ENV COSMOS_CONTAINER_NAME="crashes"
ENV COSMOS_ACCOUNT="https://crashbox-db.documents.azure.com:443/"
ENV COSMOS_PRIMARY_KEY="71IHnsVO2RQeTyF03783tTWIB7TBF1dzDTWjkhNSUlIsgYLkITTzquiRqSgopLZ7f0ZJsz48pVAb0BFqL05lng=="
ENV CosmosDBConnection="AccountEndpoint=https://crashbox-db.documents.azure.com:443/;AccountKey=71IHnsVO2RQeTyF03783tTWIB7TBF1dzDTWjkhNSUlIsgYLkITTzquiRqSgopLZ7f0ZJsz48pVAb0BFqL05lng==;"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CrashBox.WebApi.dll"]
