---
title: Prerequisites
---
### Prerequisites for Windows

To successfully run Gibbon Git Server on Windows, you need to meet the following requirements:

#### 1. Operating System

- **Windows Server** (recommended): Windows Server 2016 or higher.
- **Windows 10/11**: If using a development or small-scale setup, a modern desktop version of Windows is also sufficient.

#### 2. .NET Core Runtime

Gibbon Git Server is built using **ASP.NET Core**, so you need the appropriate runtime installed.

- Download and install the latest **ASP.NET Core Runtime** (version 8 or higher) from the [official .NET download page](https://dotnet.microsoft.com/download/dotnet/8.0).
- Ensure the runtime is available in your system’s environment variables. You can check this by opening a terminal and running:
  ```bash
  dotnet --version
  ```

You should see something like `8.x.x` if the runtime is properly installed.

### 3a. IIS (Internet Information Services)

If you want to host Gibbon Git Server on IIS (Internet Information Services), follow these steps:

- Open **Server Manager** on your Windows Server machine.
- Go to **Manage > Add Roles and Features**.
- Select **Web Server (IIS)** and proceed with the installation, making sure the following features are enabled:
    
    - **Web Server** > **Common HTTP Features** (Static Content, Default Document, etc.)
    - **Application Development** > **.NET Core Hosting Bundle** and **ASP.NET 4.x** (needed for IIS integration with ASP.NET Core)
    - **Management Tools** > **IIS Management Console**
    
    After installing IIS, download and install the **.NET Core Hosting Bundle** from [Microsoft](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-aspnetcore-8.0-windows-hosting-bundle-installer).

### 3b. Kestrel (Optional)

For lightweight or cross-platform hosting, Gibbon Git Server can also be hosted with **Kestrel**, a web server that runs natively in .NET Core. Kestrel comes built-in with ASP.NET Core, so no extra installation is required if you're using it instead of IIS.

- By default, Gibbon Git Server will use Kestrel if no IIS is configured.
- Kestrel is optimal for scenarios where you don't need a full IIS setup or for testing/development purposes.

### 5. Git Command-Line Tools

Ensure the **Git command-line tools** are installed on the machine for proper Git operations (clone, push, pull, etc.).

- Download the latest version of **Git for Windows** from [git-scm.com](https://git-scm.com/).
- After installation, verify Git is installed by running the following command in the terminal:

```
git --version
```

- You should see something like `git version 2.x.x`.

### 6. Database

Gibbon Git Server requires a database for storing configuration and user data. On Windows, the default configuration uses **SQLite**, but you can switch to another supported database if needed. If you're using the default SQLite:

- No additional setup is required for SQLite as it is embedded.

If you want to use **SQL Server** (although it’s not officially supported anymore), you would need an active SQL Server instance and adjust the connection strings in `appsettings.json` accordingly.

---

Once all prerequisites are installed, you can proceed with the [installation steps](#installation-steps) to set up and run Gibbon Git Server on your Windows machine.