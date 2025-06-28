FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:faa2daf2b72cbe787ee1882d9651fa4ef3e938ee56792b8324516f5a448f3abe AS build

WORKDIR /app

COPY ./Split.slnx ./global.json ./Directory.Packages.props ./Directory.Build.props ./.gitignore ./.editorconfig ./

COPY ./Domain/Primitives/Primitives.csproj ./Domain/Primitives/packages.lock.json ./Domain/Primitives/
COPY ./Domain/User/User.csproj ./Domain/User/packages.lock.json ./Domain/User/
COPY ./Domain/Transaction/Transaction.csproj ./Domain/Transaction/packages.lock.json ./Domain/Transaction/
COPY ./Infrastructure/Repositories/Repositories.csproj ./Infrastructure/Repositories/packages.lock.json ./Infrastructure/Repositories/
COPY ./Infrastructure/Swish/Swish.csproj ./Infrastructure/Swish/packages.lock.json ./Infrastructure/Swish/
COPY ./Application/ServiceDefaults/ServiceDefaults.csproj ./Application/ServiceDefaults/packages.lock.json ./Application/ServiceDefaults/
COPY ./Application/Web/Web.csproj ./Application/Web/packages.lock.json ./Application/Web/

RUN dotnet restore ./Application/Web/Web.csproj --locked-mode

COPY ./Domain/Primitives ./Domain/Primitives
COPY ./Domain/User ./Domain/User
COPY ./Domain/Transaction ./Domain/Transaction
COPY ./Infrastructure/Repositories ./Infrastructure/Repositories
COPY ./Infrastructure/Swish ./Infrastructure/Swish
COPY ./Application/ServiceDefaults ./Application/ServiceDefaults
COPY ./Application/Web ./Application/Web

RUN dotnet build ./Application/Web/Web.csproj --no-restore --configuration Release

RUN dotnet publish ./Application/Web/Web.csproj --no-build --configuration Release --output /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0@sha256:1e12c265e1e1b3714c5805ab0cab63380eb687b0a04f3b3ef3392494a6122614 AS release

WORKDIR /app

COPY --from=build /out ./

ENTRYPOINT ["./Split.Application.Web"]
