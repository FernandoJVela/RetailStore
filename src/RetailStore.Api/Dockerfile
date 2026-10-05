FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/RetailStore.Api/RetailStore.Api.csproj src/RetailStore.Api/
COPY src/RetailStore.Infrastructure/RetailStore.Infrastructure.csproj src/RetailStore.Infrastructure/
COPY src/RetailStore.SharedKernel/RetailStore.SharedKernel.csproj src/RetailStore.SharedKernel/
RUN dotnet restore src/RetailStore.Api/RetailStore.Api.csproj

COPY src/ src/
RUN dotnet publish src/RetailStore.Api/RetailStore.Api.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "RetailStore.Api.dll"]