# ==========================================
# 1. Build Stage (Node.js 20 Alpine)
# ==========================================
FROM node:20-alpine AS build
WORKDIR /app

# Sao chép package.json và cài dependencies trước
COPY package*.json ./
RUN npm ci

# Sao chép source code và build production
COPY . .
# Cho phép override VITE_API_BASE_URL khi build (mặc định dùng relative path /api cho Nginx reverse proxy)
ARG VITE_API_BASE_URL=/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN npm run build

# ==========================================
# 2. Production Stage (Nginx Alpine)
# ==========================================
FROM nginx:alpine AS final

# Sao chép cấu hình Nginx custom
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Sao chép bản build tĩnh từ stage 1 vào thư mục phục vụ web của Nginx
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 67

ENTRYPOINT []
CMD ["nginx", "-g", "daemon off;"]
