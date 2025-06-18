FROM node:alpine

WORKDIR /app

# Installing dependencies first (leveraging cache)
COPY package*.json ./
RUN npm install

# Install serve package globally
RUN npm install -g serve

# Now copy rest of the files and build
COPY . .
RUN npm run build

# Expose port 3000 (serve's default port)
EXPOSE 3000

# Use serve to host the static files
CMD ["serve", "-s", "dist"]
