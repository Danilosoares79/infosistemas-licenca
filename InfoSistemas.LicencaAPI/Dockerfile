FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5100

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar projetos necessarios
COPY ["InfoSistemas.LicencaAPI/InfoSistemas.LicencaAPI.csproj", "InfoSistemas.LicencaAPI/"]
RUN dotnet restore "InfoSistemas.LicencaAPI/InfoSistemas.LicencaAPI.csproj"

COPY ["InfoSistemas.LicencaAPI/", "InfoSistemas.LicencaAPI/"]
WORKDIR "/src/InfoSistemas.LicencaAPI"
RUN dotnet build "InfoSistemas.LicencaAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InfoSistemas.LicencaAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5100
ENTRYPOINT ["dotnet", "InfoSistemas.LicencaAPI.dll"]
