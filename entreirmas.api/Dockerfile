# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore EntreIrmas.sln
RUN dotnet publish EntreIrmas.Api/EntreIrmas.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

ARG VERSAO=0.0.0
ARG COMMIT=local
ARG DATA_BUILD=
RUN printf '{"versao":"%s","commit":"%s","data":"%s"}\n' "$VERSAO" "$COMMIT" "$DATA_BUILD" > /app/publish/about-info.json

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
ENV TZ="America/Sao_Paulo" \
    ASPNETCORE_URLS="http://+:8080"
WORKDIR /app
COPY --from=build /app/publish .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "EntreIrmas.Api.dll"]
