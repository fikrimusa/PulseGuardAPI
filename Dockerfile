FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PulseGuard.Api.csproj", "./"]
RUN dotnet restore "PulseGuard.Api.csproj"

COPY . .
RUN dotnet publish "PulseGuard.Api.csproj" --configuration Release --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PulseGuard.Api.dll"]
