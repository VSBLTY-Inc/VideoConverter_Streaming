#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VideoConverter_Streaming.csproj", "."]
RUN dotnet restore "./VideoConverter_Streaming.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "VideoConverter_Streaming.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoConverter_Streaming.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
RUN apt update && apt-get install -y ffmpeg
WORKDIR /app
RUN mkdir /opt/Vsblty && mkdir /opt/Vsblty/videos && mkdir /opt/Vsblty/convertedVideos && mkdir /opt/Vsblty/processedVideos && RUN chmod -R 777 /opt/Vsblty/
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoConverter_Streaming.dll"]