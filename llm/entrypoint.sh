#! /bin/sh

# Start server in background
ollama serve &

# Wait for server to become healthy
until ollama ps 2> /dev/null
do
  echo "Waiting for ollama server to be up..."
  sleep 5
done

# Now pull the model
ollama pull phi3:3.8b

# Keep server up
wait
