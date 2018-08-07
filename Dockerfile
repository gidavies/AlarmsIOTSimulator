
FROM microsoft/dotnet:2.0-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.0-sdk AS build
WORKDIR /src
COPY alarms.csproj alarms/
RUN dotnet restore alarms/alarms.csproj
WORKDIR /src/alarms
COPY . .
RUN dotnet build alarms.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish alarms.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "alarms.dll"]
