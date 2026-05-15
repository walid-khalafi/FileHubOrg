# FileHubOrg: Secure Internal File Management

![FileHubOrg Logo](https://via.placeholder.com/150/007bff/ffffff?text=FileHubOrg) <!-- Placeholder for your actual logo -->

FileHubOrg is a robust, secure, and user-friendly internal file management system designed for organizations. It empowers authenticated users to seamlessly upload, organize, share, and access files while maintaining strict control over permissions and providing comprehensive audit trails. Built with a focus on modularity and scalability, FileHubOrg aims to streamline collaboration and enhance data security within your organization.

---

## ✨ Key Features

- **Secure File Upload & Access:** Easily upload files and manage access permissions.
- **Intuitive User Interface:** A modern, responsive web UI built with ASP.NET Core (Razor Pages/MVC) and Bootstrap 5, ensuring a smooth user experience on all devices.
- **Organization by Labels:** Categorize and manage files using customizable labels for better organization.
- **Departmental Collaboration:** Browse and share files within specific departments, enhancing team collaboration.
- **User-Friendly Sharing:** Share files directly with coworkers, with clear tracking of who has access.
- **Role-Based Access Control (RBAC):** Granular permissions for users, department managers, and administrators.
- **Windows Authentication:** Seamless integration with Active Directory for secure and simplified user authentication.
- **Comprehensive Audit Logging:** Track all significant actions like uploads, downloads, deletions, and sharing activities for security and compliance.
- **Configurable Settings:** Easily manage file size limits, allowed file types, and other application settings.
- **Scalable Architecture:** Designed with clean separation of concerns (Web, Application, Domain, Infrastructure layers) for maintainability and future expansion.

## 🔒 Secure File Access

Files are never exposed through direct public URLs.

All file downloads are protected via JWT-based authorization and validated server-side access control checks.

This ensures:
- Secure authenticated access
- Prevention of unauthorized file sharing
- Fine-grained permission enforcement
- Protection against direct file enumeration
---

## 🛠️ Technology Stack

- **Backend:**
    - [ASP.NET Core 9.0](https://learn.microsoft.com/en-us/aspnet/core/) (Razor Pages / MVC)
    - [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) for data access
    - SQL Server / PostgreSQL / SQLite (configurable)
    - ASP.NET Core Identity (with Windows Authentication)
- **Frontend:**
    - Bootstrap 5 (for responsive UI and components)
    - Font Awesome (for icons)
    - JavaScript (Vanilla JS, Fetch API)
- **Architecture:**
    - Clean Architecture principles (Domain, Application, Infrastructure, Presentation layers)

---

## 🚀 Getting Started

Follow these steps to set up and run the project locally:

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/walid-khalafi/FileHubOrg.git
    cd FileHubOrg
    ```

2.  **Set up the database:**
    *   Ensure you have SQL Server (or your chosen database) installed.
    *   Configure the connection string in `appsettings.json` or `appsettings.Development.json`.
    *   Apply database migrations:
        ```bash
        dotnet ef database update
        ```
    *   *Note: You might need to install EF Core tools: `dotnet tool install --global dotnet-ef`*

3.  **Configure Authentication:**
    *   Ensure Windows Authentication is enabled in your project if you intend to use it. This typically involves changes in `Program.cs` and `launchSettings.json`.
    *   For development, you might need to configure local user accounts or impersonation.

4.  **Run the application:**
    ```bash
    dotnet run
    ```
    The application should be accessible at `http://localhost:5000` (or the port specified in `launchSettings.json`).

---

## 🤝 Contributing

Contributions are welcome! Whether it's reporting bugs, suggesting features, or submitting pull requests, your input is valuable. Please refer to the `CONTRIBUTING.md` file for guidelines.

---

## 📜 License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

---

## ❓ Questions or Feedback?

Feel free to reach out via [GitHub Issues](https://github.com/your-username/FileHubOrg/issues) or contact the project maintainers.
