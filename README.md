# Web-Based Claims Processing System (WCPS)

A full-stack web application built with ASP.NET Core MVC, designed to digitize and streamline the employee expense reimbursement process. This project replaces a manual, paper-based system with a secure, efficient, and user-friendly web portal.

![All Claims Page Screenshot]<img width="3830" height="2165" alt="image" src="https://github.com/user-attachments/assets/9c6cfed1-566a-45ac-a43a-1c19e7944982" />

---

## 🚀 Key Features

This application is a complete, end-to-end solution with a rich feature set for both employees and administrators.

* **Authentication & Authorization**: Secure user registration and login system powered by **ASP.NET Core Identity**.
* **Role-Based Access Control**: Two distinct user roles (**Employee** and **Manager**) with different permissions and views.
* **Role-Specific Dashboards**:
    * **Employee Dashboard**: Shows a personalized welcome, a count of pending claims, and quick links to common actions.
    * **Admin Dashboard**: Provides a system-wide overview, including total pending claims and registered user counts.
* **Full CRUD for Claims**: Employees can Create, Read, Update (while pending), and Delete their expense claims.
* **File Uploads**: Users can attach supporting documents (receipts, invoices) to their claims.
* **Admin Processing Workflow**: Managers can view, search, and filter all claims from all users. They can **Approve** or **Reject** pending claims.
* **Business Logic Engine**: Automatically applies a reimbursement cap when a claim is approved.
* **Automated Email Notifications**: Uses the **SendGrid API** to automatically notify employees when their claim status is updated.
* **User Management**: A secure section for admins to view, edit, and delete user accounts.
* **Reporting & Auditing**:
    * A dynamic pie chart showing the distribution of claim statuses.
    * A full **Audit Trail** that logs all major create and update events in the database.
* **Polished & Responsive UI**:
    * A custom, modern UI theme built with Bootstrap 5 and custom CSS.
    * Server-side **pagination and searching** on all data tables for excellent performance.
    * Professional UI components like notifications, modals, and custom badges.

---

## 💻 Technology Stack

* **Backend**: C#, ASP.NET Core MVC (.NET 8)
* **Database**: Entity Framework Core (Code-First), SQL Server LocalDB
* **Frontend**: HTML, CSS, JavaScript, Bootstrap 5, Chart.js
* **Security**: ASP.NET Core Identity (Authentication, Authorization, Roles)
* **Email**: SendGrid API
* **Version Control**: Git & GitHub

---

## ⚙️ Setup and Running the Project

To run this project locally, you will need:
* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* [Visual Studio 2022](https://visualstudio.microsoft.com/)
* SQL Server (LocalDB is installed with Visual Studio)

**Steps:**

1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/YourUsername/ClaimsProcessingSystem.git](https://github.com/YourUsername/ClaimsProcessingSystem.git)
    ```
2.  **Configure User Secrets (API Key):**
    * This project uses the .NET Secret Manager to securely store the SendGrid API key. You will need to get a free API key from [SendGrid](https://sendgrid.com/).
    * Open a terminal in the project's root directory and run the following command, replacing `YOUR_KEY` with your actual key:
        ```bash
        dotnet user-secrets set "SendGridKey" "YOUR_KEY"
        ```
    * *(Note: The email "from" address is hard-coded in `Services/EmailSender.cs`. You may need to change it to your verified SendGrid sender email.)*
3.  **Create the Database:**
    * In Visual Studio, open the **Package Manager Console** (`View > Other Windows > Package Manager Console`).
    * Run the following command to create and seed the database with the default admin user:
        ```powershell
        Update-Database
        ```
4.  **Run the Application:**
    * Press **F5** or the "Start Debugging" button in Visual Studio.

---

## 🔑 Login Credentials

You can use the following pre-seeded accounts to test the application.

* **Manager / Admin Account:**
    * **Email:** `admin@test.com`
    * **Password:** `Password123!`

* **Sample User Accounts:**
    * **Email:** `user2@test.com`
    * **Password:** `User@2`
    * **Email:** `sukhi.m2409@gmail.com`
    * **Password:** `Sukhi@24`
