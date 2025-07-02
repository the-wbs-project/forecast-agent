import type { Context } from "../../config";
import type { DataDogService } from "./data-dog.service";
import type { Logger } from "./logger.service";

export class HttpLogger implements Logger {
	constructor(private readonly ctx: Context) {}

	private get datadog(): DataDogService {
		return this.ctx.var.datadog;
	}

	trackRequest(duration: number): void {
		if (this.ctx.req.method === "OPTIONS") return;

		this.datadog.appendLog({
			...this.basics({ duration, resStatus: this.ctx.res.status }),
			status: "Info",
			message: `Request, ${this.ctx.req.method}, ${this.ctx.req.url} (${duration}ms)`,
		});
	}

	trackEvent(
		message: string,
		status: "Error" | "Info" | "Warn" | "Notice",
		data?: Record<string, unknown>,
	): void {
		this.datadog.appendLog({
			...this.basics({ data }),
			status,
			message: message,
		});
	}

	trackException(
		message: string,
		exception: Error,
		data?: Record<string, unknown>,
	): void {
		console.error(message);
		console.error(exception.message);
		this.datadog.appendLog({
			...this.basics({
				data: {
					data,
					message: exception ? exception.message : "No Exception Provided",
					stack: exception
						? exception.stack?.toString()
						: "No Exception Provided",
				},
			}),
			status: "Error",
			message: message,
		});
	}

	trackDependency(
		url: string,
		method: string,
		duration: number,
		request: Request,
		response?: Response,
	): void {
		this.datadog.appendLog({
			...this.basics({
				duration,
				request,
				resStatus: response?.status,
				data: {
					dependency: {
						status: response ? response.status : null,
						url,
						method,
						duration,
					},
				},
			}),
			status: "Info",
			message: `Dependency, ${method}, ${url} (${duration}ms)`,
		});
	}

	private basics(info: {
		resStatus?: number | undefined;
		duration?: number | undefined;
		request?: { url: string; method: string; headers: Headers };
		data?: Record<string, unknown> | undefined;
	}): Record<string, unknown> {
		if (!info.request) info.request = this.ctx.req.raw;

		this.ctx.req.raw.headers;

		const url = new URL(this.ctx.req.url);
		const cf: Record<string, unknown> =
			(info.request as { cf?: Record<string, unknown> })?.cf || {};

		return {
			ddsource: "worker",
			ddtags: `env:${String(this.ctx.env.DATADOG_ENV || "development")},app:forecast-agent`,
			hostname: String(this.ctx.env.DATADOG_HOST || "localhost"),
			service: "forecast-agent-worker",
			session_id: info.request.headers.get("dd_session_id"),
			http: {
				url: info.request.url,
				status_code: info.resStatus,
				method: info.request.method,
				userAgent: info.request.headers.get("User-Agent"),
				version: cf.httpProtocol,
				url_details: {
					host: url.host,
					port: url.port,
					path: url.pathname,
				},
			},
			network: {
				client: {
					geoip: {
						country: {
							iso_code: cf.country,
						},
						continent: {
							code: cf.continent,
						},
						city: {
							name: cf.city,
						},
					},
				},
			},
			usr: {
				id: this.ctx.var.user?.userId,
				email: this.ctx.var.user?.email,
			},
			...(info.duration
				? {
						performance: {
							duration: info.duration * 1000000, // convert to nanoseconds
						},
					}
				: {}),
			...(info.data
				? {
						data: info.data,
					}
				: {}),
		};
	}
}
