#!/bin/bash

# Configuration
SERVER_USER="polo"
SERVER_IP="192.168.1.44"
DEST_DIR="~/socmed-deploy"
SSH_KEY="/home/polo/.ssh/id_ed25519_server"

echo "🧹 Resetting database on $SERVER_IP..."

ssh -i "$SSH_KEY" $SERVER_USER@$SERVER_IP << 'EOF'
  set -e
  cd ~/socmed-deploy
  echo "Stopping containers and removing volumes..."
  docker compose down -v
  echo "Starting containers (this will recreate the database and run migrations)..."
  docker compose up -d
EOF

echo "✅ Database reset complete!"
