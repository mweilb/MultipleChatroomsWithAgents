Certainly! Here's **README 2: Setup Guide** in full Markdown format (from start to end), as requested.

---

# **README-Setup.md: Setup Guide**

This document will guide you through the steps required to set up the system, including installing necessary dependencies, configuring services, and preparing the environment for use.

---

## **1. Install Ollama**

Ollama is an open-source tool for natural language processing and chat-based interaction used in this system. Follow these steps to install Ollama.

### **1.1 Install Ollama**

1. **For macOS**:
   - Install via **Homebrew**:
     ```bash
     brew install ollama
     ```

2. **For Linux**:
   - Follow the installation instructions on the [official Ollama GitHub page](https://github.com/ollama/ollama).

3. **For Windows**:
   - Download the **Windows installer** from [ollama.com](https://ollama.com/docs) and follow the setup instructions.

### **1.2 Run Ollama**

Once installed, start the Ollama service by running:
```bash
ollama serve
```
The Ollama server will run locally at `http://localhost:11434`.

### **1.3 Test Ollama**

To verify that Ollama is running correctly, send a request to the endpoint:
```bash
curl http://localhost:11434
```
If Ollama is running properly, it should respond with information about the service.

---

## **2. Azure Setup**

This system integrates with **Azure Cognitive Search** and **Azure OpenAI** services. Here’s how to set them up.

### **2.1 Set up Azure Cognitive Search**

1. **Create an Azure Cognitive Search Service**:
   - Log in to the [Azure Portal](https://portal.azure.com/).
   - Search for **Azure Cognitive Search** and create a new service.
   - Specify the necessary configurations like **name**, **subscription**, and **resource group**.
   - After creation, navigate to the **Keys** section and note down the **Search Endpoint** and **Search Key**.

2. **Configure Azure Cognitive Search**:
   - Add the following configuration to your `appsettings.json` or `.env` file:
   ```json
   {
       "AZURE_SEARCH_ENDPOINT": "<Your Azure Cognitive Search Endpoint>",
       "AZURE_SEARCH_KEY": "<Your Azure Cognitive Search Key>"
   }
   ```

### **2.2 Set up Azure OpenAI**

1. **Create an Azure OpenAI Service**:
   - Go to **Azure OpenAI** in the Azure portal and create a new service.
   - Once created, obtain the **API Key** and **Endpoint** from the **Keys** section.

2. **Configure Azure OpenAI**:
   - Add the following settings to your `appsettings.json` or `.env` file:
   ```json
   {
       "AZURE_OPENAI_API_KEY": "<Your Azure OpenAI API Key>",
       "AZURE_OPENAI_ENDPOINT": "<Your Azure OpenAI Endpoint>",
       "AZURE_OPENAI_DEPLOYMENT": "<Your Azure OpenAI Deployment Name>"
   }
   ```

### **2.3 Configure Azure in the Code**

Ensure that the `AzureKernelHelper` class is properly configured with the environment variables or configuration file details to allow the system to utilize **Azure OpenAI** and **Cognitive Search**.

### Quarant setup

1. Setup Docker
https://docs.docker.com/engine/install/

2. Install qdrant/qdrant  
docker pull qdrant/qdrant

### Pinecone Setup
1. Setup Docker
https://docs.docker.com/engine/install/

2. Install pinecone 
https://docs.pinecone.io/guides/operations/local-development
docker pull ghcr.io/pinecone-io/pinecone-index:latest

### Start your vector DB

 There is script to help you start the pinecone or qdrant solution
 in VectorDB folder
   >startVectorDB <qdrant|pipecone> 

 Will start the right version with the correct default values