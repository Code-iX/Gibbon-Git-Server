---
title: Authentication
---
Gibbon Git Server currently supports **Cookie Authentication**, providing a simple and familiar login experience for users. This method uses an authentication cookie to maintain session persistence after the user has successfully logged in via a username and password form.

### 1. Cookie Authentication

Cookie Authentication allows users to log in using a web form where they provide their username and password. Once authenticated, the server creates a secure cookie that validates the user's identity for subsequent requests, eliminating the need to re-enter credentials.

#### How to Configure Cookie Authentication

To enable Cookie Authentication in Gibbon Git Server, you'll need to adjust the configuration in `appsettings.json`. Unlike the previous **Bonobo Git Server** setup, Gibbon no longer uses `web.config`. Instead, you can configure authentication directly in `appsettings.json`.

Here’s how to set it up:

1. Open the `appsettings.json` file located in the root of the published application.
2. Find the **Authentication** section and ensure the following settings are configured for Cookie Authentication:

   ```json
   {
     "Authentication": {
       "DefaultScheme": "Cookies",
       "Schemes": {
         "Cookies": {
           "LoginPath": "/Account/Login",
           "LogoutPath": "/Account/Logout",
           "ExpireTimeSpan": "00:30:00",  // Adjust the session expiration time if needed
           "SlidingExpiration": true       // Optional: extend session if user is active
         }
       }
     }
   }
```

3. Save the file and restart the application.

#### Security Considerations

Since **Git** communicates with the server using the **Basic Authentication** protocol, user credentials are sent with each request. To ensure security, it's recommended to enable **SSL** (HTTPS) in your IIS or Kestrel server to encrypt the communication.

##### Enabling SSL (HTTPS) in IIS

1. Obtain an SSL certificate for your server.
2. In **IIS Manager**, select your website, then click on **Bindings**.
3. Add a new binding for **https** and select your SSL certificate.
4. Restart your site.

Once SSL is enabled, your application will enforce secure communication, protecting user credentials during transmission.

---

This authentication mechanism is simple, secure (when combined with HTTPS), and well-suited for most environments. For more advanced authentication methods, stay tuned for future updates in Gibbon Git Server's feature set.