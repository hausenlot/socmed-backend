#!/bin/bash

# Configuration
SERVER_USER="polo"
SERVER_IP="192.168.1.44"
DEST_DIR="~/socmed-deploy"

# Paths to your local projects (Relative to this script if possible, or absolute)
BACKEND_DIR="./socmed-backend"
FRONTEND_DIR="../../CLIENT FOR ASP/social-media-frontend"

echo "🚀 Starting Social Media deployment to $SERVER_IP..."

# 1. Create destination directory
ssh $SERVER_USER@$SERVER_IP "mkdir -p $DEST_DIR/social-media-frontend"

# 2. Sync Backend files
echo "📦 Syncing Backend..."
rsync -avz --exclude 'bin' --exclude 'obj' --exclude '.git' --exclude 'socmed.db*' \
      "$BACKEND_DIR/" $SERVER_USER@$SERVER_IP:$DEST_DIR/socmed-backend/

# 3. Sync Frontend files
echo "📦 Syncing Frontend..."
rsync -avz --exclude 'node_modules' --exclude '.git' --exclude 'dist' \
      "$FRONTEND_DIR/" $SERVER_USER@$SERVER_IP:$DEST_DIR/social-media-frontend/

# 4. Sync Docker Compose file
echo "📦 Syncing Docker Compose..."
rsync -avz ./docker-compose.yml $SERVER_USER@$SERVER_IP:$DEST_DIR/

# 5. Run Docker commands on the server via SSH
echo "🏗️  Starting containers on server..."
ssh $SERVER_USER@$SERVER_IP << 'EOF'
  set -e
  cd ~/socmed-deploy

  docker compose up --build -d
  docker image prune -f
EOF

echo "✅ Deployment complete! Visit http://$SERVER_IP"
