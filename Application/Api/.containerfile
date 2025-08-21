FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:ae000be75dac94fc40e00f0eee903289e985995cc06dac3937469254ce5b60b6 AS build

WORKDIR /app

COPY ./Split.slnx ./global.json ./Directory.Packages.props ./Directory.Build.props ./.gitignore ./.editorconfig ./

COPY ./Domain/Primitives/Primitives.csproj ./Domain/Primitives/packages.lock.json ./Domain/Primitives/
COPY ./Domain/User/User.csproj ./Domain/User/packages.lock.json ./Domain/User/
COPY ./Domain/Transaction/Transaction.csproj ./Domain/Transaction/packages.lock.json ./Domain/Transaction/
COPY ./Infrastructure/Repositories/Repositories.csproj ./Infrastructure/Repositories/packages.lock.json ./Infrastructure/Repositories/
COPY ./Infrastructure/Encryptor/Encryptor.csproj ./Infrastructure/Encryptor/packages.lock.json ./Infrastructure/Encryptor/
COPY ./Infrastructure/PhoneNumberVerifier/PhoneNumberVerifier.csproj ./Infrastructure/PhoneNumberVerifier/packages.lock.json ./Infrastructure/PhoneNumberVerifier/
COPY ./Application/ServiceDefaults/ServiceDefaults.csproj ./Application/ServiceDefaults/packages.lock.json ./Application/ServiceDefaults/
COPY ./Application/Api/Api.csproj ./Application/Api/packages.lock.json ./Application/Api/

RUN dotnet restore ./Application/Api/Api.csproj --locked-mode

COPY ./Domain/Primitives ./Domain/Primitives
COPY ./Domain/User ./Domain/User
COPY ./Domain/Transaction ./Domain/Transaction
COPY ./Infrastructure/Repositories ./Infrastructure/Repositories
COPY ./Infrastructure/Encryptor ./Infrastructure/Encryptor
COPY ./Infrastructure/PhoneNumberVerifier ./Infrastructure/PhoneNumberVerifier
COPY ./Application/ServiceDefaults ./Application/ServiceDefaults
COPY ./Application/Api ./Application/Api

RUN dotnet build ./Application/Api/Api.csproj --no-restore --configuration Release

RUN dotnet publish ./Application/Api/Api.csproj --no-build --configuration Release --output /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0@sha256:515c08e27cf8d933af442f0e1b6c92cc728acb71f113ca529f56c79cb597adb5 AS release

WORKDIR /app

COPY --from=build /out ./

ENTRYPOINT ["./Split.Application.Api"]
