import { Hono } from "hono";
import { cors } from "hono/cors";
import { logger } from "hono/logger";
import type { Variables } from "./config";
import { authenticateUser } from "./middleware/auth";
import { ddLogger } from "./middleware/logger";
import {
	AUTH_ROUTES,
	PROJECTS_ROUTES,
	TASKS_ROUTES,
	WEATHER_ROUTES,
} from "./routes";
import {
	AuthService,
	DataDogService,
	Fetcher,
	HttpLogger,
	KVAuthDataService,
	KVProjectDataService,
	KVTaskDataService,
	KVWeatherDataService,
	OpenWeatherMapService,
	WeatherGuardApiService,
	createKVService,
} from "./services";
import type { WeatherApiService } from "./services/api-services/weather-api.service";

const app = new Hono<{ E: Env; Variables: Variables }>();

// Middleware
app.use("*", async (ctx, next) => {
	const env = ctx.env as Env;

	const kvService = createKVService(env.METADATA_KV);
	const authDataService = new KVAuthDataService(kvService);
	const authService = new AuthService(authDataService, env.JWT_SECRET);

	// Initialize API service for fallback
	const weatherGuardApiService = env.API_BASE_URL
		? new WeatherGuardApiService(env.API_BASE_URL, env.API_TOKEN)
		: undefined;

	// Initialize data services with API fallback
	const projectService = new KVProjectDataService(
		kvService,
		weatherGuardApiService,
	);
	const taskService = new KVTaskDataService(kvService, weatherGuardApiService);
	const weatherService = new KVWeatherDataService(
		kvService,
		weatherGuardApiService,
	);

	const datadog = new DataDogService(env);
	const loggerService = new HttpLogger(ctx);
	const fetcher = new Fetcher(loggerService);
	const weatherApiService = env.OPENWEATHER_API_KEY
		? new OpenWeatherMapService(env.OPENWEATHER_API_KEY, fetcher)
		: null;

	ctx.set("kvService", kvService);
	ctx.set("authService", authService);
	ctx.set("projectService", projectService);
	ctx.set("taskService", taskService);
	ctx.set("weatherService", weatherService);
	ctx.set("datadog", datadog);
	ctx.set("logger", loggerService);
	ctx.set("fetcher", fetcher);
	ctx.set("weatherApiService", weatherApiService as WeatherApiService | null);
	ctx.set("weatherGuardApiService", weatherGuardApiService);

	await next();
});
app.use("*", logger());
app.use("*", ddLogger);
app.use(
	"*",
	cors({
		origin: ["http://localhost:3000", "http://localhost:5173"],
		allowMethods: ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
		allowHeaders: ["Content-Type", "Authorization"],
		credentials: true,
	}),
);

// Health check
app.get("/health", (c) => {
	return c.json({
		success: true,
		message: "WeatherGuard Worker is healthy",
		data: {
			timestamp: new Date().toISOString(),
			version: "1.0.0",
		},
	});
});

const api = new Hono<{ E: Env; Variables: Variables }>()
	.use("*", authenticateUser)
	.route("/auth", AUTH_ROUTES)
	.route("/projects", PROJECTS_ROUTES)
	.route("/tasks", TASKS_ROUTES)
	.route("/weather", WEATHER_ROUTES);

app.route("/api", api);

export default {
	fetch: app.fetch,
} satisfies ExportedHandler<Env>;
