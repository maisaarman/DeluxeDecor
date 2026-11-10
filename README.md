
# Deluxe Decor — Product Catalog (Blazor + SQLite)

A multilingual product catalog website supporting Arabic, English, and French, with an admin dashboard for managing categories and products, uploading images, and receiving customer inquiries.

## Requirements
- .NET 8 SDK or later.

## Getting Started

### 1. Initialize User Secrets

Run this command once:

```bash
dotnet user-secrets init
```

### 2. Set Admin Password

```bash
dotnet user-secrets set "Admin:Password" "your-password-here"
```

### 3. Run the Project

```bash
dotnet run
```

- **Website:** Open the URL displayed in the terminal. The application automatically redirects to `/ar`.
- **Admin Dashboard:** `/admin`
- **Username:** `admin`
- **Password:** The password configured using User Secrets.
- **Database:** `App_Data/shop.db`. It is automatically created and seeded with sample data on the first run.

## Technologies Used

- Blazor Web App (.NET 8).
- Entity Framework Core.
- SQLite.
- C# and Razor Components.
- HTML and CSS.
- Image storage in `wwwroot/uploads`.
- Multilingual support using a separate `ProductTranslations` table.
- Cookie-based authentication.

## Database Design

- `Products`: Stores product information.
- `ProductTranslations`: Stores product names and descriptions in different languages.
- `Categories`: Stores product categories.
- Products are linked to categories, while translations are linked to products through `ProductId`.

## Features

- Browse products and filter them by category.
- Support for Arabic, English, and French.
- View detailed product information.
- Add, edit, and delete products through the admin dashboard.
- Manage product categories.
- Upload and manage product images.
- Receive customer inquiries through the contact button.

## Project Structure

```text
DeluxeDecor/
├── Components/
│   ├── Layout/
│   └── Pages/
├── Data/
├── Services/
├── App_Data/
│   └── shop.db
├── wwwroot/
│   └── uploads/
├── Program.cs
├── appsettings.json
└── README.md
```