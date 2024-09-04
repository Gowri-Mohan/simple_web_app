FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5211
#EXPOSE 7057

ENV ASPNETCORE_URLS=http://+:5211
#ENV ASPNETCORE_URLS=https://+:7057

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["MyAppAPI/MyAppAPI.csproj", "MyAppAPI/"]
RUN dotnet restore "MyAppAPI/MyAppAPI.csproj"
COPY . .
WORKDIR "/src/MyAppAPI"
RUN dotnet build "MyAppAPI.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "MyAppAPI.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyAppAPI.dll"]
