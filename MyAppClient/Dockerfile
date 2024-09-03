FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5210
EXPOSE 7058

ENV ASPNETCORE_URLS=http://+:5210
ENV ASPNETCORE_URLS=https://+:7058

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["MyAppClient/MyAppClient.csproj", "MyAppClient/"]
RUN dotnet restore "MyAppClient/MyAppClient.csproj"
COPY . .
WORKDIR "/src/MyAppClient"
RUN dotnet build "MyAppClient.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "MyAppClient.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyAppClient.dll"]
