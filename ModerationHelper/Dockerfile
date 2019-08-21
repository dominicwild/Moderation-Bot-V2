FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["ModerationHelper/ModerationHelper.csproj", "ModerationHelper/"]
RUN dotnet restore "ModerationHelper/ModerationHelper.csproj"
COPY . .
WORKDIR "/src/ModerationHelper"
RUN dotnet build "ModerationHelper.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ModerationHelper.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ModerationHelper.dll"]