#!/bin/bash
set -e
SSH_KEY="/home/polo/.ssh/id_ed25519_server"
SERVER_USER="polo"
SERVER_IP="192.168.1.44"

echo "🗑️ Wiping Postgres database on $SERVER_IP..."
ssh -i "$SSH_KEY" $SERVER_USER@$SERVER_IP << 'EOF'
  set -e
  cd ~/postgres
  docker compose down
  docker run --rm -v /home/polo/postgres:/data alpine sh -c 'rm -rf /data/postgres_data'
  docker compose up -d
  cd ~/socmed-deploy
  docker compose restart socmed-backend
EOF
echo "✅ Remote database wiped and backend restarted!"
