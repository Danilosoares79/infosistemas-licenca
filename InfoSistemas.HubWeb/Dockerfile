FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5200

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["InfoSistemas.HubWeb/InfoSistemas.HubWeb.csproj", "InfoSistemas.HubWeb/"]
COPY ["InfoSistemas.LicencaAPI/InfoSistemas.LicencaAPI.csproj", "InfoSistemas.LicencaAPI/"]
RUN dotnet restore "InfoSistemas.HubWeb/InfoSistemas.HubWeb.csproj"

COPY ["InfoSistemas.HubWeb/", "InfoSistemas.HubWeb/"]
COPY ["InfoSistemas.LicencaAPI/", "InfoSistemas.LicencaAPI/"]

WORKDIR "/src/InfoSistemas.HubWeb"
RUN dotnet build "InfoSistemas.HubWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InfoSistemas.HubWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5200
ENTRYPOINT ["dotnet", "InfoSistemas.HubWeb.dll"]
