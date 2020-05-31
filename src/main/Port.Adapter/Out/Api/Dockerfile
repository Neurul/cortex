FROM microsoft/dotnet:2.0-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.0-sdk AS build
WORKDIR /src
COPY *.sln ./
COPY . .
RUN dotnet restore neurUL-Cortex-Master.sln
WORKDIR /src/main/Port.Adapter/Out/Api
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app /property:PublishWithAspNetCoreTargetManifest=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "neurUL.Cortex.Port.Adapter.Out.Api.dll"]
