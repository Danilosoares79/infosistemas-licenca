FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["InfoSistemas.HubWeb.csproj", "."]
RUN dotnet restore "InfoSistemas.HubWeb.csproj"

COPY . .
RUN dotnet publish "InfoSistemas.HubWeb.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5200
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5200
ENTRYPOINT ["dotnet", "InfoSistemas.HubWeb.dll"]
