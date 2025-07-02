import type { Logger } from './logging/logger.service';

export class Fetcher {
	constructor(private readonly logger: Logger) {}

	async fetch(
		url: string,
		options?: RequestInit,
	): Promise<Response> {
		const start = new Date();
		const method = options?.method || 'GET';

		try {
			const response = await fetch(url, options);
			const duration = Math.abs(new Date().getTime() - start.getTime());

			// Create a minimal request object for logging
			const request = new Request(url, options);
			
			this.logger.trackDependency(
				url,
				method,
				duration,
				request,
				response,
			);

			return response;
		} catch (error) {
			const duration = Math.abs(new Date().getTime() - start.getTime());
			
			this.logger.trackException(
				`Failed to fetch ${method} ${url}`,
				error as Error,
				{ url, method, duration }
			);
			
			throw error;
		}
	}
}