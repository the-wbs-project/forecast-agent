import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";
import { RouterModule } from "@angular/router";

@Component({
	selector: "app-navigation",
	standalone: true,
	imports: [CommonModule, RouterModule],
	templateUrl: "./navigation.html",
	styleUrl: "./navigation.scss",
})
export class NavigationComponent {}
