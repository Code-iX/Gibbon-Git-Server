---
title: FAQ
---

### How to support Gibbon Git Server?
You can:
- Contribute
- Promote on your blog
- Tweet about it
- Answer a question on StackOverflow
- Recommend it to your friend or colleague
- Donate

### How to clone a repository?
1. Go to the Repository Detail.
2. Copy the value in the Git Repository Location. It should look like: `http://servername/projectname.git`.
3. Go to your command line and run: `git clone http://servername/projectname.git`.

### How do I change my password?
1. Click on the account settings in the top right corner.
2. Enter new password and confirmation.
3. Save changes.

### How to backup data?
1. Go to the installation folder of Gibbon Git Server on the server. The default location is: `C:\inetpub\wwwroot\Gibbon.Git.Server`.
2. Copy the content of the `App_Data` folder to your backup directory.
3. If you changed the location of your repositories, back them up as well.

### How to change repositories folder?
1. Log in as an administrator.
2. Go to **Global Settings**.
3. Set the desired value for the **Repository Directory**.
4. Ensure the directory exists on the hard drive, and that the IIS User has proper permissions to modify the folder.
5. Save changes.

### Can I allow anonymous access to a repository?
1. Edit the desired repository (or do this when creating the repository).
2. Check the **Anonymous** checkbox.
3. Save changes.

To allow anonymous push, modify the global settings:
1. Log in as an administrator.
2. Go to **Global Settings**.
3. Check the value **Allow push for anonymous repositories**.
4. Save changes.

### fatal: http: /info/refs not valid: is this a git repository?
This is a git client way of saying that it didn’t receive a git stream as a response from a server. This usually means that there has been an error on the server side.

To determine the error type, view the log file located at `App_Data/Bonobo.Git.Server.Errors.log`.

### Gibbon Git Server doesn’t serve CSS
This is a common issue for Windows 8 users. To resolve it:
1. Go to **Turn Windows Features On or Off** screen.
2. Navigate to **Internet Information Services (IIS) -> World Wide Web Services -> Common HTTP Features**.
3. Tick **Static Content**.

### Cloning Error - RPC failed
This error can have multiple causes, but the most frequent ones are related to the size of the request. If you encounter this issue, try increasing the following values:
- Run `git config http.postBuffer [desired size]` on your client, try `524288000`.
- Increase `<requestLimits maxAllowedContentLength="[desired size]">` in the `web.config`; try `1073741824`.
- Increase `<httpRuntime maxRequestLength="[desired size]">` in the `web.config`; try `1024000`.

Note: On IIS 8.0, you may not need to limit `maxRequestLength`. If you encounter issues, try removing the line from the `web.config`.

### SSL and large repositories
If you experience issues while using SSL and pushing large repositories, increase the variable sizes as described above. If that doesn’t help, apply the following Microsoft patch: **(KB2634328)**.

### Error 500.19 and file execution issues on IIS 8
To resolve these issues, run the following commands:
```bash
%windir%\system32\inetsrv\appcmd unlock config -section:system.webServer/handlers
%windir%\system32\inetsrv\appcmd unlock config -section:system.webServer/modules
