import { Hono } from "hono";
import type { Variables } from "../config";
import { Http } from "../services";

const projects = new Hono<{ E: Env; Variables: Variables }>();

projects
	.get("/", Http.projects.getProjectsAsync)
	.get("/:id", Http.projects.getProjectByIdAsync)
	.post("/", Http.projects.createProjectAsync)
	.put("/:id", Http.projects.updateProjectAsync)
	.delete("/:id", Http.projects.deleteProjectAsync);

export const PROJECTS_ROUTES = projects;
