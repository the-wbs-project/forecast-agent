import { Hono } from "hono";
import type { Variables } from "../config";
import { Http } from "../services";

const weather = new Hono<{ E: Env; Variables: Variables }>();

weather
	.get("/forecast", Http.weather.getForecastAsync)
	.get("/analyses/project/:projectId", Http.weather.getAnalysesByProjectAsync)
	.get("/analyses/:id", Http.weather.getAnalysisByIdAsync)
	.get("/analyses/high-risk", Http.weather.getHighRiskAnalysesAsync)
	.post("/analyses", Http.weather.createAnalysisAsync)
	.post(
		"/analyses/project/:projectId/generate",
		Http.weather.generateAnalysisAsync,
	)
	.put("/analyses/:id", Http.weather.updateAnalysisAsync);

export const WEATHER_ROUTES = weather;
