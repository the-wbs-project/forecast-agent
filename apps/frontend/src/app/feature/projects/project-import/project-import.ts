import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UploaderModule } from '@syncfusion/ej2-angular-inputs';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ProjectService } from '../services/project';

@Component({
  selector: 'app-project-import',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    UploaderModule,
    TextBoxModule,
    ButtonModule
  ],
  templateUrl: './project-import.html',
  styleUrl: './project-import.scss'
})
export class ProjectImportComponent {
  private router = inject(Router);
  private projectService = inject(ProjectService);
  
  protected readonly selectedFile = signal<File | null>(null);
  projectName = '';
  description = '';
  protected readonly importing = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal(false);

  handleFileSelection(args: any) {
    if (args.filesData && args.filesData.length > 0) {
      this.selectedFile.set(args.filesData[0].rawFile);
      if (!this.projectName && args.filesData[0].name) {
        this.projectName = args.filesData[0].name.replace(/\.[^/.]+$/, '');
      }
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1048576) return Math.round(bytes / 1024) + ' KB';
    return Math.round(bytes / 1048576) + ' MB';
  }

  async importProject() {
    if (!this.selectedFile() || !this.projectName) {
      this.error.set('Please select a file and enter a project name');
      return;
    }

    this.importing.set(true);
    this.error.set(null);
    this.success.set(false);

    try {
      await this.projectService.importProject(
        this.selectedFile()!,
        this.projectName,
        this.description || undefined
      );
      
      this.success.set(true);
      setTimeout(() => {
        this.router.navigate(['/projects']);
      }, 1500);
    } catch (error) {
      this.error.set(error instanceof Error ? error.message : 'Failed to import project');
    } finally {
      this.importing.set(false);
    }
  }

  navigateBack() {
    this.router.navigate(['/projects']);
  }
}