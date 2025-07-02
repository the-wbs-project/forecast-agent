export class DataDogService {
	private readonly logs: unknown[] = [];

	constructor(private readonly env: Env) {}

	appendLog(log: unknown): void {
		this.logs.push(log);
	}

	async flush(): Promise<void> {
		try {
			await fetch('https://http-intake.logs.us5.datadoghq.com/api/v2/logs', {
				method: 'POST',
				body: JSON.stringify(this.logs),
				headers: {
					Accept: 'application/json',
					'Content-Type': 'application/json',
					'DD-API-KEY': this.env.DATADOG_API_KEY,
				},
			});
		} catch (e) {
			//@ts-ignore
			console.log(e.message);
		}
	}
}