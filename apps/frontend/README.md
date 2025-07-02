# Weather Guard Frontend

Angular 20 frontend application for the Weather Guard construction project management system.

## Features

- **Project Management**: Create, view, edit, and delete construction projects
- **Project Import**: Import projects from MS Project, Primavera P6, and other formats
- **Task Management**: Manage project tasks with weather sensitivity tracking
- **Modern UI**: Built with Syncfusion UI components and Bootstrap 5
- **Server-Side Rendering**: Optimized performance with Angular SSR
- **Signals**: Uses Angular's latest signals for reactive state management

## Tech Stack

- Angular 20 with standalone components
- TypeScript
- Server-Side Rendering (SSR)
- Syncfusion UI Library
- Bootstrap 5
- Minimal RxJS usage (as requested)

## Getting Started

### Prerequisites

- Node.js 18+ 
- npm or pnpm

### Installation

```bash
npm install
```

### Development

Run the development server:

```bash
npm start
```

Navigate to `http://localhost:4200/`. The application will automatically reload if you change any source files.

### Build

Build the project for production:

```bash
npm run build
```

The build artifacts will be stored in the `dist/` directory.

### Running SSR Build

After building, you can run the SSR server:

```bash
npm run serve:ssr:frontend
```

## Project Structure

```
src/
├── app/
│   ├── components/
│   │   ├── projects/
│   │   │   ├── project-list.component.ts
│   │   │   ├── project-form.component.ts
│   │   │   ├── project-import.component.ts
│   │   │   └── project-detail.component.ts
│   │   └── shared/
│   │       └── navigation.component.ts
│   ├── models/
│   │   ├── project.model.ts
│   │   ├── task.model.ts
│   │   └── common.model.ts
│   ├── services/
│   │   ├── project.service.ts
│   │   └── task.service.ts
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   ├── app.config.ts
│   ├── app.routes.ts
│   └── app.ts
├── styles.scss
└── index.html
```

## Available Routes

- `/` - Redirects to projects list
- `/projects` - List all projects
- `/projects/create` - Create a new project
- `/projects/import` - Import a project file
- `/projects/:id` - View project details
- `/projects/:id/edit` - Edit project

## API Integration

The frontend expects the API to be running at `http://localhost:5000/api` in development mode. Update the `environment.ts` file to change the API endpoint.

## Notes

- Authentication is not included as requested
- The application uses Angular signals for state management instead of RxJS observables where possible
- All components are standalone for better tree-shaking and lazy loading
- Syncfusion components are used for complex UI elements like grids, date pickers, and charts
- Bootstrap provides the base styling framework