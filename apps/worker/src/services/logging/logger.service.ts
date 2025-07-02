export interface Logger {
	trackRequest(duration: number): void;
	trackEvent(
		message: string,
		status: "Error" | "Info" | "Warn" | "Notice",
		data?: Record<string, unknown>,
		ddsource?: string,
		service?: string,
	): void;
	trackException(
		message: string,
		exception: Error,
		data?: Record<string, unknown>,
	): void;
	trackDependency(
		url: string,
		method: string,
		duration: number,
		request: Request,
		response?: Response,
	): void;
}
