import { Hono } from "hono";
import type { Variables } from "../config";
import { Http } from "../services";

const tasks = new Hono<{ E: Env; Variables: Variables }>();

tasks
	.get("/project/:projectId", Http.tasks.getTasksAsync)
	.get("/schedule/:scheduleId", Http.tasks.getTasksByScheduleAsync)
	.get("/:id", Http.tasks.getTaskByIdAsync)
	.get("/:id/children", Http.tasks.getTasksByParentAsync)
	.get(
		"/project/:projectId/weather-sensitive",
		Http.tasks.getWeatherSensitiveTasksAsync,
	)
	.post("/", Http.tasks.createTaskAsync)
	.put("/:id", Http.tasks.updateTaskAsync)
	.delete("/:id", Http.tasks.deleteTaskAsync);

export const TASKS_ROUTES = tasks;
