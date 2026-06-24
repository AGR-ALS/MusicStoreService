FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

USER root

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        fontconfig \
        wget \
        cabextract \
        xfonts-utils && \
    wget https://downloads.sourceforge.net/corefonts/arial32.exe -O /tmp/arial32.exe && \
    mkdir -p /usr/share/fonts/truetype/msttcorefonts && \
    cabextract -F '*.TTF' -d /usr/share/fonts/truetype/msttcorefonts /tmp/arial32.exe && \
    fc-cache -f -v && \
    rm -rf /tmp/* && \
    apt-get clean

    
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MusicStoreShowcase.Main/MusicStoreShowcase.Main.csproj", "MusicStoreShowcase.Main/"]
COPY ["MusicStoreShowcase.Infrastructure/MusicStoreShowcase.Infrastructure.csproj", "MusicStoreShowcase.Infrastructure/"]
COPY ["MusicStoreShowcase.Application/MusicStoreShowcase.Application.csproj", "MusicStoreShowcase.Application/"]
COPY ["MusicStoreShowcase.Domain/MusicStoreShowcase.Domain.csproj", "MusicStoreShowcase.Domain/"]
RUN dotnet restore "MusicStoreShowcase.Main/MusicStoreShowcase.Main.csproj"
COPY . .
WORKDIR "/src/MusicStoreShowcase.Main"
RUN dotnet build "MusicStoreShowcase.Main.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MusicStoreShowcase.Main.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MusicStoreShowcase.Main.dll"]
