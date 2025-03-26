param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("qdrant", "pinecone")]
    [string]$dbType
)

switch ($dbType) {
    "qdrant" {
        # Check if the container "qdrant-multi-agents" exists.
        $id = docker ps -aq -f name=qdrant-multi-agents
        if ([string]::IsNullOrWhiteSpace($id)) {
            Write-Output "Container not found. Running new qdrant container..."
            # Adjust the path as needed; here we use the script's directory as the workspace folder.
            docker run --name qdrant-multi-agents -d -p 6333:6333 -p 6334:6334 -v ".\qdrant\.qdrant_data:/qdrant/storage" qdrant/qdrant
        }
        else {
            Write-Output "Container exists. Starting qdrant container..."
            docker start qdrant-multi-agents
        }
    }
    "pinecone" {
        # Check if the container "pinecone-local" exists.
        $id = docker ps -aq -f name=pinecone-local
        if ([string]::IsNullOrWhiteSpace($id)) {
            Write-Output "Container not found. Running new pinecone container..."
            docker run -d `
                --name pinecone-local `
                -e PORT=5080 `
                -e PINECONE_HOST=localhost `
                -p 5080-5090:5080-5090 `
                --platform linux/amd64 `
                ghcr.io/pinecone-io/pinecone-local:latest
        }
        else {
            Write-Output "Container exists. Starting pinecone container..."
            docker start pinecone-local
        }
    }
}
