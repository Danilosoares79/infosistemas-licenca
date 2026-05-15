FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5200

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["InfoSistemas.HubWeb.csproj", "."]
RUN dotnet restore "InfoSistemas.HubWeb.csproj"

COPY . .
RUN dotnet publish "InfoSistemas.HubWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5200
ENTRYPOINT ["dotnet", "InfoSistemas.HubWeb.dll"]
