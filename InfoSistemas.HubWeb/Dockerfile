FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["InfoSistemas.HubWeb.csproj", "."]
RUN dotnet restore "InfoSistemas.HubWeb.csproj"

COPY . .
RUN dotnet publish "InfoSistemas.HubWeb.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "InfoSistemas.HubWeb.dll"]
