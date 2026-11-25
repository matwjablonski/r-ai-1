FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY ServiceDesk.Api.csproj ./
RUN dotnet restore ServiceDesk.Api.csproj
COPY . ./
RUN dotnet publish ServiceDesk.Api.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS dev
WORKDIR /src
COPY ServiceDesk.Api.csproj ./
RUN dotnet restore ServiceDesk.Api.csproj
COPY . ./
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
CMD ["dotnet", "watch", "run", "--no-launch-profile", "--urls", "http://0.0.0.0:8080"]

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS test
WORKDIR /src
COPY . ./
RUN dotnet restore ServiceDesk.slnx
CMD ["dotnet", "test", "ServiceDesk.slnx", "--no-restore", "--logger", "trx;LogFileName=backend.trx", "--results-directory", "/results"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS production
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ServiceDesk.Api.dll"]
