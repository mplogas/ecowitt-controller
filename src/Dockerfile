#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80   
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV ASPNETCORE_HTTPS_PORTS=443
ENV ASPNETCORE_HTTP_PORTS=80

ADD Ecowitt.Controller/ /src/Ecowitt.Controller
WORKDIR "/src/Ecowitt.Controller"
RUN dotnet restore "./Ecowitt.Controller.csproj"
RUN dotnet build "./Ecowitt.Controller.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/Ecowitt.Controller"
RUN dotnet publish "./Ecowitt.Controller.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecowitt.Controller.dll"]