# 1️⃣ Build stage
FROM node:20.11.1-slim AS builder

# Set working directory
WORKDIR /app

# Install dependencies
COPY package*.json ./
RUN npm ci

# Build application
COPY . .
RUN npm run build

# 2️⃣ Serve with Nginx
FROM nginx:alpine AS production

# Remove default nginx config
RUN rm -rf /etc/nginx/conf.d/* 

# Provide custom config if needed
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy build output to nginx directory
COPY --from=builder /app/dist /usr/share/nginx/html

# Expose port
EXPOSE 80

CMD ["nginx","-g","daemon off;"]
