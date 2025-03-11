# Build stage
FROM node:20 AS build
WORKDIR /app
COPY ["dotsocialnetwork.client/package.json", "dotsocialnetwork.client/yarn.lock", "./"]
RUN npm install
COPY ["dotsocialnetwork.client/", "."]
RUN npm build

# Final stage with Nginx
FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY ["dotsocialnetwork.client/nginx.conf", "/etc/nginx/conf.d/default.conf"]
EXPOSE 80 443
CMD ["nginx", "-g", "daemon off;"]