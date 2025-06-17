# 1. Build stage
FROM node:alpine AS builder

WORKDIR /app

# Installing dependencies first (leveraging cache)
COPY package*.json ./
RUN npm install

# Now copy rest of the files and build
COPY . .
RUN npm run build

# 2. Serve with Nginx
FROM nginx:alpine

# Remove the default config
RUN rm -f /etc/nginx/conf.d/default.conf

# Provide custom config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built files from builder
COPY --from=builder /app/dist /usr/share/nginx/html

# Expose port 80
EXPOSE 80

CMD ["nginx","-g","daemon off;"]
