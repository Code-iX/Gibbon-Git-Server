---
title: FAQ
---

## Table of Contents
1. [How to support Gibbon Git Server](#how-to-support-gibbon-git-server)
2. [Repository Management](#repository-management)
   - [How do I clone a repository?](#how-do-i-clone-a-repository)
   - [How to change repositories folder?](#how-to-change-repositories-folder)
   - [Why is it no longer possible to change the repository folder in the UI?](#why-is-it-no-longer-possible-to-change-the-repository-folder-in-the-ui)
   - [Can I allow anonymous access to a repository?](#can-i-allow-anonymous-access-to-a-repository)
3. [Password and Account](#password-and-account)
   - [How do I change the password?](#how-do-i-change-the-password)
4. [Server Issues](#server-issues)
   - [How do I back up my data?](#how-do-i-back-up-my-data)
   - [Gibbon Git Server doesn’t serve CSS](#gibbon-git-server-doesnt-serve-css)
   - [SSL and large repositories](#ssl-and-large-repositories)
   - [Error 500.19 and file execution issues on IIS 8](#error-50019-and-file-execution-issues-on-iis-8)
5. [Common Errors](#common-errors)
   - [fatal: http: /info/refs not valid: is this a git repository?](#fatal-http-inforefs-not-valid-is-this-a-git-repository)
   - [Cloning Error - RPC failed](#cloning-error---rpc-failed)
6. [Git Hooks and Username](#git-hooks-and-username)

---

## How to support Gibbon Git Server?

You can:
- Contribute
- Promote on your blog
- Tweet about it
- Answer a question on StackOverflow
- Recommend it to your friend or colleague
- Donate

---

## Repository Management

### How do I clone a repository?

1. Go to the **Repository Details** page.
2. Copy the **Git Repository Location**.
3. Select one of the following:
   - Open your git application of choice and clone the repository.
   - Run the following in your terminal:  
     `git clone http://your-server-name/repository-name.git`

---

### How to change repositories folder?

1. **Stop the server**.
2. Edit the **`appsettings.json`** file.
3. Update the **`RepositoryPath`** key with the new path.
4. Move existing repositories to the new folder (if applicable).
5. **Restart the server**.

---

### Why is it no longer possible to change the repository folder in the UI?

This feature was removed because the repository is linked to the database, and a simple switch could cause issues. If there is a strong reason to change the repository folder, it's best to do this while the server is not running and make the change consciously in the `appsettings.json` file.

---

### Can I allow anonymous access to a repository?

- For allowing anonymous access to a repository, follow these steps:
  1. Edit the desired repository (or do this when creating the repository).
  2. Check the **Anonymous** checkbox.
  3. Save.
- To allow anonymous push:
  1. Log in as an administrator.
  2. Go to **Server Settings**.
  3. Check the value **Allow push for anonymous repositories**.
  4. Save changes.

---

## Password and Account

### How do I change the password?

1. Navigate to the **Account Settings** under your username in the top right corner.
2. Go to the **Password** tab.
3. Enter your old password and the new one with confirmation.
4. Click **Save**.

---

## Server Issues

### How do I back up my data?

- Copy the **App_Data** folder and your repositories into a backup directory.
- Alternatively, use the **Database Backup** feature in the **Server Site**.

---

### Gibbon Git Server doesn’t serve CSS

This is a common issue for Windows 8 users. To resolve it:

1. Go to **Turn Windows Features On or Off** screen.
2. Navigate to **Internet Information Services (IIS) -> World Wide Web Services -> Common HTTP Features**.
3. Tick **Static Content**.

---

### SSL and large repositories

If you experience issues while using SSL and pushing large repositories, increase the variable sizes as described above in the "Cloning Error - RPC failed" section. If that doesn’t help, apply the following Microsoft patch: **(KB2634328)**.

---

### Error 500.19 and file execution issues on IIS 8

To resolve these issues, run the following commands:

```bash
%windir%\system32\inetsrv\appcmd unlock config -section:system.webServer/handlers
%windir%\system32\inetsrv\appcmd unlock config -section:system.webServer/modules
```
---

## Common Errors

### fatal: http: /info/refs not valid: is this a git repository?

This error occurs when the git client doesn’t receive a proper git stream as a response from the server. This typically indicates a server-side issue.

To diagnose the error, check the log file located at `App_Data/Bonobo.Git.Server.Errors.log`.

---

### Cloning Error - RPC failed

This error often relates to the size of the request. To resolve this, try increasing the following values:

- On your client, run:  
  `git config http.postBuffer [desired size]`, try `524288000` (500MB).
  
- In your `web.config`, increase:  
  `<requestLimits maxAllowedContentLength="[desired size]">`, try `1073741824` (1GB).
  
- Also in `web.config`, increase:  
  `<httpRuntime maxRequestLength="[desired size]">`, try `1024000` (1GB).

**Note:** On IIS 8.0, `maxRequestLength` may not need to be limited. If you experience issues, try removing this line from the `web.config`.

---

### SSL and large repositories

If you're encountering issues while using SSL and pushing large repositories, follow the steps described in the "Cloning Error - RPC failed" section to increase the size limits.

If the problem persists, apply the Microsoft patch: **(KB2634328)**.

---

### Error 500.19 and file execution issues on IIS 8

To resolve this issue, execute the following commands in the command prompt:

```bash
%windir%\system32\inetsrv\appcmd unlock config -section:system.webServer/handlers
%windir%\system32\inetsrv\appcmd unlock config -section:system.webServer/modules
```
This will unlock the configuration sections for handlers and modules in IIS 8.

---

## Git Hooks and Username

### I'd like to use git hooks to restrict access. How do I access the web frontend username?

Bonobo provides the following environment variables to help manage access via git hooks:

- **`AUTH_USER`**: The username used to log in. This variable will be empty if the operation (e.g., clone, push, pull) was done anonymously.
- **`REMOTE_USER`**: Same as `AUTH_USER`.
- **`AUTH_USER_TEAMS`**: A comma-separated list containing all the teams the user belongs to.
- **`AUTH_USER_ROLES`**: A comma-separated list containing all the roles assigned to the user.
- **`AUTH_USER_DISPLAYNAME`**: This displays the user's full name (Given Name + Surname), if available. Otherwise, it will display the username.

**Important Note:** Due to the way HTTP basic authentication works, if anonymous operations (such as push or pull) are enabled, the variables mentioned above will always be empty. This means that you won't be able to retrieve the user details for anonymous requests.
