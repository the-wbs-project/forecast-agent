import { AuthHttpService } from "./auth.http-service";
import { ProjectsHttpService } from "./projects.http-service";
import { TasksHttpService } from "./tasks.http-service";
import { WeatherHttpService } from "./weather.http-service";

export const Http = {
	auth: new AuthHttpService(),
	projects: new ProjectsHttpService(),
	tasks: new TasksHttpService(),
	weather: new WeatherHttpService(),
};
