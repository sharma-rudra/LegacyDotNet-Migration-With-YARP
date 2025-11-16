## Migration Playbook: A Procedural Guide for Migrating .NET 4.8 Applications to .NET 8 via an Incremental YARP Proxy Strategy

### 1. Introduction: The Strategy

The migration of a legacy .NET Framework application represents a high-risk endeavor. A comprehensive "big bang" rewrite methodology is characterized by significant capital outlay, extended development timelines, and a delayed realization of value, which is postponed until project completion.

This document delineates a more secure, incremental methodology based on the **Strangler Fig Pattern**. This strategy employs a .NET 8 application as the primary ingress point, or "front door," facilitating the phased migration of the legacy application in discrete, vertical slices (feature-by-feature).

The principal technology utilized is **YARP (Yet Another Reverse Proxy)**, a .NET 8 library developed by Microsoft. This tool enables the new application to function as an intelligent, highly configurable reverse proxy.

The operational flow is as follows:
1.  All incoming HTTP traffic is first intercepted by the .NET 8 YARP proxy.
    
2.  Should a request target functionality not yet migrated (e.g., `/Blog`), YARP is configured to forward this request to the legacy .NET 4.8 application.
    
3.  Conversely, if the request pertains to a feature that has been successfully migrated (e.g., `/Home`), the .NET 8 application will process it directly.
    
4.  Incrementally, an increasing number of routes are natively handled by the .NET 8 application. This process continues until the legacy application is fully supplanted, or "strangled," and can be decommissioned.


## 2. Phase 0: Initial Project Setup
<img width="528" height="55" alt="1" src="https://github.com/user-attachments/assets/38870e84-7aad-4d2b-af14-be9b5d977191" />


As a prerequisite, both the legacy and new applications must be configured to run concurrently.

1.  **Legacy Application (.NET 4.8):** Execute the existing `BasicBlog` project. It is necessary to record its operational URL (e.g., `http://localhost:54321`), which will serve as the "backend" destination.
    
2.  **New Proxy Application (.NET 8):**
    
    -   Instantiate a new **ASP.NET Core Empty** project within the solution (e.g., `BasicBlog.Yarp`).
        
    -   It must be confirmed that this application operates on a distinct URL (e.g., `http://localhost:5000`), which will function as the new primary ingress point.
        

## 3. Phase 1: Configure the YARP Front Door (Proxy All Traffic)

The initial objective is to configure the .NET 8 application to proxy all incoming traffic to the legacy application.

### 1. YARP Package Installation

Within the **.NET 8 project**, execute the following command to install the YARP package:

```
dotnet add package Yarp.ReverseProxy

```

### 2. `appsettings.json` Configuration

Modify the `appsettings.json` file in the **.NET 8 project** to include a `ReverseProxy` configuration. This new section must define:

-   **Routes:** A "catch-all" route, specified by the path `"{**catch-all}"`, to capture all inbound requests.
    
-   **Clusters:** A designated cluster that specifies the `Address` of the running .NET 4.8 legacy application (e.g., `http://localhost:54321/`).
    

### 3. `Program.cs` Service Configuration

In the **.NET 8 project's** `Program.cs` file, the following services and middleware must be registered:

1.  Register the YARP services with the DI container: `builder.Services.AddReverseProxy().LoadFromConfig(...)`.
    
2.  Integrate the YARP middleware into the request pipeline: `app.MapReverseProxy()`.
<img width="730" height="186" alt="7" src="https://github.com/user-attachments/assets/d9f9f0b0-59c2-44b5-af57-9ad7a4c16dab" />


    

### 4. Phase 1 Verification

Execute both applications. Navigate to the URL of the **.NET 8 application**. The expected result is the complete rendering of the legacy .NET 4.8 application, confirming the proxy is operational.

## 4. Phase 2: Implement Shared Authentication

This phase is the most critical component of the migration. It is imperative to configure both applications to read and decrypt the same authentication cookie. This is achieved by establishing a shared cryptographic key repository, or "key store."

### 1. Legacy Application Configuration (.NET 4.8)

1.  **Package Installation:** Within the **.NET 4.8 project**, install the Data Protection compatibility package. It is essential to specify a `2.x` version to maintain .NET Framework compatibility.
    
    ```
    Install-Package Microsoft.AspNetCore.DataProtection.SystemWeb -Version 2.3.0
    
    ```
    
2.  **Configuration File:** In the `App_Start` directory, create a new class, `DataProtectionConfig.cs`, which must inherit from `DataProtectionStartup`. Within this class, override the `ConfigureServices` method. This method must call `services.AddDataProtection()` and configure it with a shared `ApplicationName` (e.g., "BasicBlog") and the file path to the shared key store via `PersistKeysToFileSystem` (e.g., `@"C:\temp\basicblog-key-store"`).
    

### 2. New Application Configuration (.NET 8)

1.  **Package Installation:** In the **.NET 8 project**, install the corresponding packages.
    
    ```
    dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore -Version 8.0.6
    dotnet add package Microsoft.EntityFrameworkCore.SqlServer -Version 8.0.6
    dotnet add package Microsoft.AspNetCore.DataProtection.Extensions -Version 8.0.6
    
    ```
    
    -   The `Microsoft.AspNetCore.DataProtection.Extensions` package is particularly essential, as it provides the necessary compatibility methods.
        
2.  **Model Recreation:** Replicate the data models in the .NET 8 project. This involves creating a minimal `ApplicationUser` class inheriting from `IdentityUser` and an `ApplicationDbContext` class inheriting from `IdentityDbContext`.
    
3.  **`Program.cs` Configuration:**
    
    1.  Register the services for `DbContext` and ASP.NET Core `Identity`.
        
    2.  Invoke `builder.Services.AddDataProtection()`, ensuring the `ApplicationName` and `PersistKeysToFileSystem` path exactly match the configuration in the legacy application.
        
    3.  **It is critical to chain the `.SetCompatibilityMode(DataProtectionVersions.Version_2_0)`** method. This directive instructs the .NET 8 Data Protection stack to operate in a mode compatible with the `2.3.0` package used by the legacy application.
        
    4.  Configure the authentication cookie to use the same name as the legacy application: `options.Cookie.Name = ".AspNet.ApplicationCookie"`.
        
    5.  Integrate the authentication middleware (`app.UseAuthentication()` and `app.UseAuthorization()`) into the request pipeline **preceding** the `app.MapReverseProxy()` middleware.
        

### 3. Phase 2 Verification

Clear all browser cookies and purge the contents of the key store folder. Execute both applications and initiate a login via the .NET 8 URL. Successful authentication should result in the generation of a new key file in the shared folder, confirming that both applications share a common authentication state.
<img width="730" height="210" alt="2" src="https://github.com/user-attachments/assets/b37f6ee2-df7a-4d26-b9bf-170ceaa93b8a" />


## 5. Phase 3: Migrate a Vertical Slice (HomeController)

In this phase, the `/Home` controller will be migrated. This process involves re-routing requests for `/Home` away from the proxy to be handled natively by the .NET 8 application.

### 1. YARP Route Configuration (`appsettings.json`)

The YARP routing configuration must be modified from a "catch-all" to a specific, controller-based configuration.

1.  **Remove** the generic `"catch-all"` route.
    
2.  **Implement** new, specific routes for all controllers _not yet_ migrated (e.g., `"/Blog/{**remainder}"`, `"/Account/{**remainder}"`). This change effectively "frees" the `/Home` route, allowing it to be processed by the .NET 8 application instead of being proxied.
    

### 2. MVC Service and Middleware Configuration (`Program.cs`)

Register the necessary services and middleware for MVC and static file handling.

1.  Add `builder.Services.AddControllersWithViews()` to the service container.
    
2.  Integrate `app.UseStaticFiles()` into the pipeline, ensuring it is placed **before** `app.UseRouting()` to serve CSS and JavaScript assets.
    
3.  Map the MVC controller routes **before** mapping the YARP proxy. This sequence is critical to ensure migrated routes are handled locally.
    
    ```
    // This route handles migrated controllers
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    
    // This route handles everything else
    app.MapReverseProxy();
    
    ```
    

### 3. Static Asset Migration

1.  In the .NET 8 project, establish a `wwwroot` directory.
    
2.  Within `wwwroot`, create subdirectories for `css`, `js`, and `fonts`.
    
3.  Migrate all corresponding static assets from the legacy application's `Content`, `Scripts`, and `fonts` directories into these new `wwwroot` subdirectories.
    

### 4. View Migration and Refactoring

1.  Transfer the complete `Views` directory from the legacy project to the .NET 8 project.
    
2.  **`Views/Shared/_Layout.cshtml`**: This file requires modification. The legacy bundling helpers (`@Styles.Render`, `@Scripts.Render`) must be replaced with standard HTML `<link>` and `<script>` tags pointing to the new asset locations within `wwwroot` (e.g., `~/css/slate-bootstrap.css`).
    
3.  **`Views/Shared/Error.cshtml`**: Refactor this view. The model must be updated from `System.Web.Mvc.HandleErrorInfo` to a new, compatible .NET 8 `ErrorViewModel`.
    
4.  **`Views/Shared/_LoginPartial.cshtml`**: This view will encounter build failures due to uninitialized authentication services. As a temporary measure, simplify the view to exclusively display "Log in" and "Register" links, removing all `@inject` directives and `User.Identity` references.
    

### 5. Controller Migration

1.  Copy `HomeController.cs` into the `Controllers` directory of the .NET 8 project.
    
2.  **Refactor the Controller:** Modify the `using` statements by replacing `System.Web.Mvc` with `Microsoft.AspNetCore.Mvc`. Additionally, update the class namespace and confirm that all action methods return `IActionResult`.
    

### 6. Phase 3 Verification

Execute both applications and navigate to the .NET 8 URL.

-   Requests to `/Home/About` should now be processed by the **.NET 8 application**. This can be verified by modifying the view's content.
    
-   Requests to `/Blog` should continue to be handled by **YARP** and proxied to the legacy .NET 4.8 application.

<img width="896" height="357" alt="incremental-migration-proxy-request" src="https://github.com/user-attachments/assets/06b49734-2b33-48f5-9734-8834128fa792" />

    

## 6. The Iterative Process: Next Steps

A stable, repeatable migration pattern has now been established. The migration of subsequent controllers, such as `BlogController`, follows this procedure:

1.  **Refactor `BlogController.cs`**: Migrate the controller file to the .NET 8 project. Update its `using` statements and namespace. Refactor all data access logic, replacing Entity Framework 6 (EF6) calls (e.g., `db.Blogs...`) with their modern Entity Framework Core (EF Core) equivalents, typically via an injected `DbContext` (e.g., `_context.Blogs...`).
    
2.  **Update YARP Configuration:** In the `appsettings.json` file, **remove** the corresponding `"blog-route"` from the `ReverseProxy` configuration. This action "claims" the route for the .NET 8 application.
    
3.  **Verification:** Navigate to the `/Blog` endpoint. The request should now be processed natively by the .NET 8 application.
   <img width="630" height="55" alt="4" src="https://github.com/user-attachments/assets/3fad1b53-7230-463d-9989-7457c3845b41" />
   <img width="566" height="55" alt="5" src="https://github.com/user-attachments/assets/a6f78433-318a-41f6-b50b-be67c44409f2" />


    

This iterative process is to be repeated for all remaining controllers (e.g., `AccountController`, `ManageController`). When all routes have been removed from the YARP configuration, the proxy's role is complete, and the legacy .NET 4.8 application can be safely decommissioned.
