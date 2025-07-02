import { Routes } from '@angular/router';
import { 
  ProjectListComponent,
  ProjectFormComponent,
  ProjectImportComponent,
  ProjectDetailComponent
} from './feature/projects';

export const routes: Routes = [
  { path: '', redirectTo: '/projects', pathMatch: 'full' },
  { path: 'projects', component: ProjectListComponent },
  { path: 'projects/create', component: ProjectFormComponent },
  { path: 'projects/import', component: ProjectImportComponent },
  { path: 'projects/:id', component: ProjectDetailComponent },
  { path: 'projects/:id/edit', component: ProjectFormComponent }
];
