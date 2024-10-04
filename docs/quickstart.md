---
title: Quickstart Guide
---

Welcome to the Gibbon Git Server! Follow these steps to get your server up and running quickly.

## 1. Installation

### Prerequisites:
- Windows Server or Windows 10/11
- IIS (Internet Information Services) installed
- .NET 8.0 Runtime installed

### Steps:

1. **Download the Latest Release**: 
   Download the latest version of Gibbon Git Server from [here](https://github.com/Code-iX/Gibbon-Git-Server/releases).

2. **Extract Files**: 
   Extract the contents of the downloaded zip file to your desired directory.

3. **Configure IIS**:
   - Open IIS Manager.
   - Add a new site and point the physical path to the directory where you extracted the Gibbon Git Server files.
   - Bind the site to a hostname or IP address, and configure SSL if necessary.

4. **Configure Permissions**:
   Ensure that the application pool has the necessary permissions to read/write to the directory where the repositories and configuration files are stored.

5. **Database Setup**:
   - Gibbon Git Server uses a lightweight SQLite database by default.
   - Run the server for the first time to automatically set up the database using EF Migrations.

6. **Update `appsettings.json`**:
   Customize your configuration by modifying the `appsettings.json` file, including repository paths, server options, and email settings (if notifications are used).

## 2. Initial Setup

1. **Open the Web Interface**: 
   Navigate to the URL where your Gibbon Git Server is hosted (e.g., `http://localhost`).

2. **Create Admin User**:
   - The first time you access the server, you'll be prompted to create an admin user.
   - Provide the necessary details and create your admin account.

3. **Set Up User Roles**:
   - After logging in, navigate to the **User Management** section.
   - Create additional users and assign them appropriate roles (Admin, Member, Viewer).

## 3. Create Your First Repository

1. **Go to Repository Management**:
   - In the admin interface, navigate to the **Repositories** section.
   
2. **Add a New Repository**:
   - Click **Create New Repository**.
   - Provide a name and description for the repository.
   - Configure the visibility (Public or Private) and assign user permissions.

3. **Clone the Repository**:
   - Clone your new repository using Git on your local machine:
     ```bash
     git clone http://<your-server-url>/<repository-name>.git
     ```

4. **Push Your Code**:
   - Add files to the repository, commit them, and push to the server:
     ```bash
     git add .
     git commit -m "Initial commit"
     git push origin main
     ```

## 4. Manage Users and Permissions

1. **User Management**:
   - In the **User Management** section, add new users to the system.
   - Assign roles such as Admin, Member, or Viewer, depending on their access needs.

2. **Repository Permissions**:
   - Define user-specific permissions for each repository (e.g., Read-only, Write access).
   - You can also set up branch-specific permissions for tighter control.

## 5. Explore Additional Features

- **Post-Commit Actions**: Set up plugins to automate tasks after each commit.
- **Branch Visualization**: Use the graphical branch view to track the development flow.
- **Advanced Search**: Utilize the built-in search functionality to find repositories, commits, and more.
- **Custom Configurations**: Modify server settings directly through the admin interface or via the database.

For more detailed documentation, check out the [full user guide](documentation).

---

By following these steps, you’ll have your Gibbon Git Server up and running, managing repositories, users, and permissions with ease!
