# syntax=docker/dockerfile:1.7
ARG DOTNET_VERSION=10.0

# --- restore stage -----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS restore
WORKDIR /src
COPY Directory.Build.props ./
COPY Sport.slnx ./
COPY src/Sport.Core/Sport.Core.csproj                               src/Sport.Core/
COPY src/Sport.Disciplines.FBL/Sport.Disciplines.FBL.csproj         src/Sport.Disciplines.FBL/
COPY src/Sport.Disciplines.BKB/Sport.Disciplines.BKB.csproj         src/Sport.Disciplines.BKB/
COPY src/Sport.Disciplines.BDM/Sport.Disciplines.BDM.csproj         src/Sport.Disciplines.BDM/
COPY src/Sport.Disciplines.VBV/Sport.Disciplines.VBV.csproj         src/Sport.Disciplines.VBV/
COPY src/Sport.Disciplines.BOX/Sport.Disciplines.BOX.csproj         src/Sport.Disciplines.BOX/
COPY src/Sport.Disciplines.ATH/Sport.Disciplines.ATH.csproj         src/Sport.Disciplines.ATH/
COPY src/Sport.Infrastructure/Sport.Infrastructure.csproj           src/Sport.Infrastructure/
COPY src/Sport.Api/Sport.Api.csproj                                 src/Sport.Api/
RUN --mount=type=cache,id=nuget-daline-api,target=/root/.nuget/packages \
    dotnet restore src/Sport.Api/Sport.Api.csproj

# --- build + publish stage ---------------------------------------------------
FROM restore AS publish
COPY src/ ./src/
RUN --mount=type=cache,id=nuget-daline-api,target=/root/.nuget/packages \
    dotnet publish src/Sport.Api/Sport.Api.csproj \
        -c Release \
        -o /app \
        --no-restore \
        /p:UseAppHost=false

# --- runtime stage -----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS runtime
RUN apk add --no-cache curl
WORKDIR /app
COPY --from=publish /app ./
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080
USER 1000:1000
HEALTHCHECK --interval=10s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "Sport.Api.dll"]
