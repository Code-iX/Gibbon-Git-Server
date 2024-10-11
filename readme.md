# Gibbon Git Server

[![GitHub Pages](https://github.com/Code-iX/Gibbon-Git-Server/actions/workflows/jekyll-gh-pages.yml/badge.svg)](https://github.com/Code-iX/Gibbon-Git-Server/actions/workflows/jekyll-gh-pages.yml)

**Gibbon Git Server** is a self-hosted Git server built with **ASP.NET Core**. This fork is currently under heavy development, and branch squashing will occur soon. Feel free to explore, but please refrain from forking for development purposes at this time.

For further information and documentation, please visit the [official website](https://code-ix.github.io/Gibbon-Git-Server/).

This is a fork of the original [Bonobo Git Server](https://github.com/jakubgarfield/Bonobo-Git-Server), which was last 
updated in 2019. The original project was built on .NET Framework, which is now outdated. This fork aims to modernize 
the project by migrating it to ASP.NET Core 8.0. The goal is to provide a more efficient, secure, and cross-platform Git 
server solution. You can find the key changes [below](#key-changes-to-bonobo-git-server).

---

## Features

- Web-based interface for managing Git repositories and users.
- Full Git support (clone, push, pull, etc.).
- Enhanced performance through .NET 8 optimizations.
- Open-source under the **MIT license**.
- Fully cross-platform thanks to .NET Core.

## Prerequisites

- **ASP.NET Core Runtime** (version 8 or higher).
- Windows Server with IIS or use **Kestrel** for cross-platform hosting.
- `git` command-line tool for Git operations.

## Installation Steps

1. **Clone the repository**:
   <code>git clone https://github.com/Code-iX/Gibbon-Git-Server.git</code>

2. **Publish the application** using the .NET Core CLI:
   <code>dotnet publish -c Release -o ./publish</code>

3. **Deploy** the published files on IIS (Windows) or any web server (e.g., Kestrel for Linux/macOS).

4. **Database Setup**:
   - Update the connection string in <code>appsettings.json</code> for your preferred database (SQLite is default).
   - Run Entity Framework migrations (optional):
     <code>dotnet ef database update</code>

5. **Run the Application**:
   - Launch the app via IIS or directly using:
     <code>dotnet Gibbon-Git-Server.dll</code>

6. **Access the Web Interface** at <code>http://localhost:5000</code> (or your configured port).

## Configuration

- Update **`appsettings.json`** to adjust application settings.
- The default database is SQLite, but you can switch to another by updating the connection string.
  
## Update

Before each update please read carefully the information about **compatibility issues** between your version and the latest one in [changelog](https://https://code-ix.github.io/Gibbon-Git-Server/changelog).

- **Backup** your data.
- **Stop the server**.
- Delete all the files in the installation folder **except App_Data**.
- Copy the files from the downloaded archive to the server location.
- **Start the server**.

## FAQ

### How do I clone a repository?

1. Go to the **Repository Details** page.
1. Copy the **Git Repository Location**.
1. Run the following in your terminal:
   `git clone http://your-server-name/repository-name.git`

### How do I change the password?

1. Navigate to the **Account Settings** under your username in the top right corner.
1. Go to the **Password** tab.
1. Enter your old password as well as the new one with confirmation.
1. Click **Save**.

### How do I back up my data?

- Copy the **App_Data** folder and your repositories into a backup directory.
- Alternatively, you can use the **Database Backup** feature in the **Server Site**.

#### How to change repositories folder?

1. **Stop the server**.
1. Edit the **`appsettings.json`** file.
1. Update the **`RepositoryPath`** key with the new path.
1. If there are existing repositories, move them to the new folder.
1. **Restart the server**.

#### Can I allow anonymous access to a repository?

- For allowing anonymous access to a repository, follow these steps:
  * Edit the desired repository (or do this when creating the repository).
  * Check **Anonymous** check box.
  * Save.
- For allowing anonymous push you have to modify global settings.
  * Log in as an administrator.
  * Go to **Server Settings**.
  * Check the value **Allow push for anonymous repositories**
  * Save changes.
                          
#### I'd like to use git hooks to restrict access. How do I access the web frontend usernam?

Bonobo provides the following environment variables:

* `AUTH_USER`: The username used to login. Empty if it was an anonymous operation (clone/push/pull)
* `REMOTE_USER`: Same as `AUTH_USER`
* `AUTH_USER_TEAMS`: A comma-separated list containing all the teams the user belongs to. 
* `AUTH_USER_ROLES`: A comma-separated list containing all the roles the user belongs to. 
* `AUTH_USER_DISPLAYNAME`: Given Name + Surname if available. Else the username.

**Beware that due to the way HTTP basic authentication works, if anonymous operations (push/pull) are enabled the variables above will always be empty!**

## Contributing

Pull requests are welcome. For significant changes, please open an issue first to discuss what you'd like to change.

## License

This project is licensed under the MIT License.

## Key changes to Bonobo Git Server

- Migrated from .NET Framework to **ASP.NET Core 8**.
- Removed support for **Active Directory (AD) login**.
- Dropped **SQL Server** support in favor of **Entity Framework migrations**.
- Updated the **UI** to a more modern look and feel.
