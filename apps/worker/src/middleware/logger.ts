import type { Context } from "../config";

export async function ddLogger(
	ctx: Context,
	next: () => Promise<void>,
): Promise<Response | undefined> {
	const start = new Date();

	await next();

	const duration = Math.abs(new Date().getTime() - start.getTime());

	const logger = ctx.var.logger;

	logger.trackRequest(duration);

	ctx.executionCtx.waitUntil(ctx.var.datadog.flush());
}
