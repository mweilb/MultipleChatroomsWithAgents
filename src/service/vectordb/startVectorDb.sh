#!/bin/bash

if [ "$#" -ne 1 ]; then
  echo "Usage: $0 [qdrant|pinecone]"
  exit 1
fi

CHOICE="$1"

case "$CHOICE" in
  qdrant)
    WORKSPACE_FOLDER="$(pwd)"
    # Check if the container exists
    if [ -z "$(docker ps -aq -f name=qdrant-multi-agents)" ]; then
      echo "Container not found. Running new qdrant container..."
      docker run --name qdrant-multi-agents -d -p 6333:6333 -v "./qdrant/.qdrant_data:/qdrant/storage" qdrant/qdrant
    else
      echo "Container exists. Starting qdrant container..."
      docker start qdrant-multi-agents
    fi
    ;;
  pinecone)
    # Check if the container exists
    if [ -z "$(docker ps -aq -f name=pinecone-local)" ]; then
      echo "Container not found. Running new pinecone container..."
      docker run -d \
        --name pinecone-local \
        -e PORT=5080 \
        -e PINECONE_HOST=localhost \
        -p 5080-5090:5080-5090 \
        --platform linux/amd64 \
        ghcr.io/pinecone-io/pinecone-local:latest
    else
      echo "Container exists. Starting pinecone container..."
      docker start pinecone-local
    fi
    ;;
  *)
    echo "Invalid parameter. Usage: $0 [qdrant|pinecone]"
    exit 1
    ;;
esac
