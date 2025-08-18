FROM node:24@sha256:d2b6b5aedb5b729f68ee1129e0f5a5d4713d93f82448249e82241876d8e8d86e AS build

WORKDIR /app

COPY ./package.json ./package-lock.json ./
RUN npm ci

COPY ./ ./
RUN npm run prepare
RUN npm run build

FROM nginx:1.29@sha256:33e0bbc7ca9ecf108140af6288c7c9d1ecc77548cbfd3952fd8466a75edefe57 AS publish

COPY ./nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/build /usr/share/nginx/html/
