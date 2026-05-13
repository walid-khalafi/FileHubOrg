# Contributing to FileHubOrg

Thank you for considering contributing to FileHubOrg! We welcome your contributions, whether it's bug reports, feature requests, documentation improvements, or code submissions.

---

## 🌟 Code of Conduct

Please read and follow our [Code of Conduct](CODE_OF_CONDUCT.md) to ensure a welcoming and inclusive environment for everyone.

---

## 💡 How to Contribute

There are several ways you can contribute to FileHubOrg:

1.  **Reporting Bugs:**
    *   If you find a bug, please check if it has already been reported.
    *   If not, open a new issue with a clear title and a detailed description.
    *   Include steps to reproduce the bug, your environment details (OS, browser, .NET version), and any relevant screenshots or logs.

2.  **Suggesting Enhancements:**
    *   Have an idea for a new feature or improvement? Open an issue to discuss it.
    *   Provide a clear description of the proposed feature and the problem it solves.

3.  **Submitting Pull Requests (Code Contributions):**
    *   **Fork the repository:** Create your own fork of the FileHubOrg repository.
    *   **Create a new branch:** Make your changes in a new branch (e.g., `feature/add-new-widget` or `bugfix/fix-login-issue`).
    *   **Write your code:** Follow the project's coding standards and best practices.
    *   **Add tests:** If applicable, write unit or integration tests for your changes.
    *   **Commit your changes:** Use clear and concise commit messages.
    *   **Push to your fork:** Push your branch to your fork on GitHub.
    *   **Open a Pull Request:** Submit a pull request against the `main` or `develop` branch of the original repository.
        *   Provide a clear title and description for your PR.
        *   Explain the changes you've made and why.
        *   Link to the relevant issue if applicable.

---

## 🚀 Development Setup

To get started with development:

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/walid-khalafi/FileHubOrg.git
    cd FileHubOrg
    ```

2.  **Set up the database:**
    *   Ensure you have [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-developer) (or your chosen database) installed.
    *   Configure the connection string in `appsettings.json` or `appsettings.Development.json`.
    *   Apply database migrations:
        ```bash
        dotnet ef database update
        ```
    *   *Note: You might need to install EF Core tools: `dotnet tool install --global dotnet-ef`*

3.  **Configure Authentication:**
    *   If using Windows Authentication, ensure it's enabled in `Program.cs` and `launchSettings.json`.
    *   For local development, you might need to set up mock users or adjust authentication settings.

4.  **Run the application:**
    ```bash
    dotnet run
    ```

---

## 📝 Coding Standards

*   Follow the established coding conventions for ASP.NET Core and C#.
*   Write clear, concise, and well-commented code.
*   Use meaningful variable and method names.
*   Adhere to the project's architectural patterns (e.g., Clean Architecture).
*   Format your code using `dotnet format` or your IDE's formatter.

---

## ✅ Testing

*   Write unit tests for business logic in the Application and Domain layers.
*   Write integration tests for API endpoints or controller actions.
*   Ensure all tests pass before submitting a pull request.

---

## 📚 Documentation

*   Keep comments up-to-date.
*   If introducing significant new features, consider updating the project's documentation.

---

## 🎉 Thank You!

We appreciate you taking the time to contribute!
