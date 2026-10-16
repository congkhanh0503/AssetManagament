#!/bin/sh
# Sinh chứng chỉ tự ký một lần; lưu trong volume /etc/nginx/certs nên giữ nguyên qua các lần cập nhật.
# SERVER_IP: địa chỉ IP mà điện thoại dùng để truy cập server.
set -e
CERT_DIR=/etc/nginx/certs
mkdir -p "$CERT_DIR"
if [ ! -f "$CERT_DIR/server.crt" ] || [ ! -f "$CERT_DIR/server.key" ]; then
  echo "[gen-cert] Tạo chứng chỉ tự ký cho ${SERVER_IP:-127.0.0.1}"
  openssl req -x509 -nodes -newkey rsa:2048 -days 3650     -keyout "$CERT_DIR/server.key" -out "$CERT_DIR/server.crt"     -subj "/CN=qlkho"     -addext "subjectAltName=IP:${SERVER_IP:-127.0.0.1},DNS:localhost"
fi
