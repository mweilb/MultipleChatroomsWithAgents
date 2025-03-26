Below is a complete set of installation instructions—mirroring the macOS guide—but tailored for a Windows environment.

---

# Complete Installation Instructions for Windows

## 1. Node.js & npm

**Purpose**  
•	Node.js is required to run JavaScript-based tools and scripts.  
•	npm (Node Package Manager) installs your project’s dependencies.

**Steps**

1. **Download & Install:**  
   •	Visit [nodejs.org](https://nodejs.org) and download the Windows installer (.msi).  
   •	Run the installer to install both Node.js and npm.

2. **Verify Installation:**  
   •	Open Command Prompt and run:  
   ```bash
   node -v
   npm -v
   ```

3. **Project Dependency Setup:**  
   •	Navigate to your project’s client folder (using File Explorer or Command Prompt) and run:
   ```bash
   cd client
   npm install
   ```

---

## 2. .NET SDK (Version 9.0)

**Purpose**  
•	The .NET SDK is needed for building and debugging .NET applications as well as powering VS Code’s .NET tooling.

**Steps**

1. **Determine Your Windows Architecture:**  
   •	Open Command Prompt and run:
   ```bash
   echo %PROCESSOR_ARCHITECTURE%
   ```  
   •	If it returns ARM64, use the ARM64 installer (for Windows on ARM). If it returns AMD64 (or similar), use the x64 installer.

2. **Download & Install:**  
   •	Visit the [.NET 9.0 SDK download page](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) and select the installer for your architecture.  
   •	Run the installer.

3. **Verify Installation:**  
   •	Open Command Prompt and run:
   ```bash
   dotnet --version
   ```

4. **Restart Visual Studio Code:**  
   •	Close VS Code completely and reopen it to ensure it recognizes the new SDK.

5. Sometimes, you need to set the policy on your machine
   In PowerShell, run 
   Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
---

## 3. Visual Studio Code

**Purpose**  
•	VS Code is your primary development environment for editing and debugging code.

**Steps**

1. **Download & Install:**  
   •	Visit [code.visualstudio.com](https://code.visualstudio.com) and download the Windows installer.  
   •	Follow the installation wizard.

2. **Install Essential Extensions for .NET:**  
   •	Open VS Code.  
   •	Press `Ctrl + Shift + X` to open the Extensions Marketplace.  
   •	Search for and install the following extensions:  
     - **C# Dev Kit** (by Microsoft)  
     - **C# Debug** (by Microsoft)  
     - *(Optional)* **C# and NuGet Package Manager**

---

## 4. Ollama with DeepSeek (r1 Release)

**Purpose**  
•	Ollama runs local AI models.  
•	DeepSeek-r1 is the specific AI model you’ll use.

**Steps**

1. **Install Ollama:**  
   •	Visit [ollama.com/download](https://ollama.com/download). If a Windows installer is available, download the Windows version.  
   •	Run the installer and follow the setup instructions.

2. **Verify Installation:**  
   •	Open Command Prompt and run:
   ```bash
   ollama --version
   ```

3. **Install the DeepSeek-r1 Model:**  
   •	Run the following command:
   ```bash
   ollama pull deepseek-r1
   ```

> **Note:** If Ollama is not yet supported on Windows, please check with the vendor for alternatives or updated instructions.

---

## 5. Start API and Client

**Steps**

1. **Start the API:**  
   •	Follow your project’s instructions to start the API. For example, open Command Prompt and run:
   ```bash
   dotnet run --project ApiProject
   ```  
   •	Replace `ApiProject` with the actual project folder or command as needed.

2. **Start the Client:**  
   •	Navigate to your project’s client folder (if not already there) and run:
   ```bash
   npm start
   ```

---

## Final Summary

1. **Node.js & npm:**  
   •	Download from [nodejs.org](https://nodejs.org), install, verify with `node -v` & `npm -v`, and run `npm install` in the client folder.

2. **.NET SDK (9.0):**  
   •	Determine your architecture using `%PROCESSOR_ARCHITECTURE%`, download from [.NET 9.0 SDK page](https://dotnet.microsoft.com/en-us/download/dotnet/9.0), install, verify with `dotnet --version`, and restart VS Code.

3. **Visual Studio Code:**  
   •	Download from [code.visualstudio.com](https://code.visualstudio.com), install, and add essential .NET extensions.

4. **Ollama with DeepSeek (r1 Release):**  
   •	Download the Windows installer (if available) from [ollama.com/download](https://ollama.com/download), install, verify with `ollama --version`, and pull the model with `ollama pull deepseek-r1`.  
   > *If Ollama is not supported on Windows yet, refer to the vendor for further guidance.*
 
5. **Optional Install a local Vector DB:**
   • Download Docker
     https://docs.docker.com/desktop/setup/install/windows-install/

     For Qdrant, https://qdrant.tech/documentation/
     > docker pull qdrant/qdrant

     For Pinecone, https://docs.pinecone.io/guides/operations/local-development
     > docker pull ghcr.io/pinecone-io/pinecone-index:latest

     In the root directory goto vectordb and run
     Run StartVectorDB.ps1 <qdrant|pinecone>

6. **Start API and Client:**  
   •	Run your API (e.g., using `dotnet run --project ApiProject`) and start the client using `npm start`.


---

Follow these instructions step by step to ensure your Windows development environment is fully set up. Let me know if you need any further assistance!