# ==========================================
# 1. Base Runtime Image (.NET 10.0)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

# ==========================================
# 2. Build & Publish Stage
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Sao chép file csproj và restore dependencies trước (tận dụng Docker cache)
COPY ["QLKho.Api.csproj", "./"]
RUN dotnet restore "QLKho.Api.csproj"

# Sao chép toàn bộ source code và build
COPY . .
RUN dotnet build "QLKho.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QLKho.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# 3. Final Production Runtime
# ==========================================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Tạo thư mục uploads để lưu trữ file PDF bàn giao
RUN mkdir -p /app/wwwroot/uploads/handovers /app/wwwroot/uploads/documents

ENTRYPOINT ["dotnet", "QLKho.Api.dll"]
