#!/bin/bash

# Configuration
SERVER_USER="polo"
SERVER_IP="192.168.1.44"
DEST_DIR="~/socmed-deploy"

# PATH TO YOUR KEY (Tell Bash to look at your Windows folder)
SSH_KEY="/home/polo/.ssh/id_ed25519_server"

# Paths to your local projects
BACKEND_DIR="./socmed-backend"
FRONTEND_DIR="../../CLIENT FOR ASP/social-media-frontend"

echo "🚀 Starting Social Media deployment to $SERVER_IP..."

# 1. Create destination directory (Added -i for the key)
ssh -i "$SSH_KEY" $SERVER_USER@$SERVER_IP "mkdir -p $DEST_DIR/social-media-frontend"

# 2. Sync Backend files (Added -e for the key)
echo "📦 Syncing Backend..."
rsync -avz -e "ssh -i $SSH_KEY" --exclude 'bin' --exclude 'obj' --exclude '.git' --exclude 'socmed.db*' \
      "$BACKEND_DIR/" $SERVER_USER@$SERVER_IP:$DEST_DIR/socmed-backend/

# 3. Sync Frontend files
echo "📦 Syncing Frontend..."
rsync -avz -e "ssh -i $SSH_KEY" --exclude 'node_modules' --exclude '.git' --exclude 'dist' \
      "$FRONTEND_DIR/" $SERVER_USER@$SERVER_IP:$DEST_DIR/social-media-frontend/

# 4. Sync Docker Compose file
echo "📦 Syncing Docker Compose..."
rsync -avz -e "ssh -i $SSH_KEY" ./docker-compose.yml $SERVER_USER@$SERVER_IP:$DEST_DIR/

# 5. Run Docker commands on the server
echo "🏗️ Starting containers on server..."
ssh -i "$SSH_KEY" $SERVER_USER@$SERVER_IP << 'EOF'
  set -e
  cd ~/socmed-deploy
  docker compose up --build -d
  docker image prune -f
EOF

echo "✅ Deployment complete! Visit http://$SERVER_IP"