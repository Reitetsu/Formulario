FROM node:20-alpine AS build
WORKDIR /app
COPY ["frontend/sysbimbo-app/package.json", "frontend/sysbimbo-app/package-lock.json", "./"]
RUN npm ci
COPY ["frontend/sysbimbo-app/", "./"]
RUN npm run build -- --configuration production

FROM nginx:alpine AS final
COPY --from=build /app/dist/sysbimbo-app/browser /usr/share/nginx/html
COPY ["frontend/sysbimbo-app/nginx.conf", "/etc/nginx/conf.d/default.conf"]
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
