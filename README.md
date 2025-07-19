# Fundoo Notes App

A Google Keep-inspired notes application built with Angular frontend and ASP.NET Core backend.

> **Note:** This application provides a complete note-taking experience similar to Google Keep with modern web technologies.

## Project Structure

```
Fundoo-Notes-App/
├── Frontend/          # Angular application
├── Backend/           # ASP.NET Core Web API
└── README.md         # This file
```

## Frontend (Angular)

The frontend is built with Angular and provides a Google Keep-like interface for managing notes.

### Features
- Create, edit, and delete notes
- Archive and trash functionality
- Color coding for notes
- Labels and reminders
- Pin/unpin notes
- Grid and list view modes
- User authentication

### Getting Started

Navigate to the Frontend directory:
```bash
cd Frontend
npm install
ng serve
```

## Backend (ASP.NET Core)

The backend provides REST API endpoints using Clean Architecture with CQRS pattern.

### Features
- JWT Authentication
- CRUD operations for notes
- Label management
- Reminder functionality
- Archive and trash operations
- SQL Server database

### Getting Started

Navigate to the Backend directory and run the application using Visual Studio or:
```bash
cd Backend
dotnet run
```

## Technologies Used

### Frontend
- Angular
- Angular Material
- TypeScript
- SCSS

### Backend
- ASP.NET Core
- Entity Framework Core
- SQL Server
- MediatR (CQRS)
- JWT Authentication

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the MIT License.
