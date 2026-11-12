# ---------- build ----------
FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
ARG CONFIGURACAO=production
RUN npx ng build --configuration ${CONFIGURACAO}

# ---------- runtime ----------
FROM nginx:alpine
COPY config/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/entreirmas-ui/browser /etc/nginx/html
EXPOSE 80
