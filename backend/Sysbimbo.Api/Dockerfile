FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/Sysbimbo.Api/Sysbimbo.Api.csproj", "backend/Sysbimbo.Api/"]
RUN dotnet restore "backend/Sysbimbo.Api/Sysbimbo.Api.csproj"
COPY ["backend/Sysbimbo.Api/", "backend/Sysbimbo.Api/"]
WORKDIR "/src/backend/Sysbimbo.Api"
RUN dotnet build "Sysbimbo.Api.csproj" -c Release -o /app/build
RUN dotnet publish "Sysbimbo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Sysbimbo.Api.dll"]
