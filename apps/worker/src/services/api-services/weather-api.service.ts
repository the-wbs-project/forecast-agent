import type { WeatherForecastDto } from "../../dto";
import type { Fetcher } from "../fetcher.service";

export interface WeatherApiService {
	getForecast(
		latitude: number,
		longitude: number,
		days: number,
	): Promise<WeatherForecastDto[]>;
	getCurrentWeather(
		latitude: number,
		longitude: number,
	): Promise<WeatherForecastDto>;
}

export class OpenWeatherMapService implements WeatherApiService {
	constructor(
		private apiKey: string,
		private fetcher: Fetcher,
	) { }

	async getForecast(
		latitude: number,
		longitude: number,
		days: number,
	): Promise<WeatherForecastDto[]> {
		const response = await this.fetcher.fetch(
			`https://api.openweathermap.org/data/2.5/forecast?lat=${latitude}&lon=${longitude}&appid=${this.apiKey}&units=metric&cnt=${days * 8}`, // 8 forecasts per day (3-hour intervals)
		);

		if (!response.ok) {
			throw new Error(`Weather API error: ${response.statusText}`);
		}

		const data: unknown = await response.json();

		// Group by day and aggregate
		const dailyForecasts: WeatherForecastDto[] = [];
		const groupedByDay = new Map<string, unknown[]>();

		// Type guard to ensure data has the expected structure
		if (
			!data ||
			typeof data !== "object" ||
			!("list" in data) ||
			!Array.isArray((data as { list: unknown[] }).list)
		) {
			throw new Error("Invalid response format from weather API");
		}

		const weatherData = data as {
			list: Array<{
				dt: number;
				main: { temp: number; humidity: number; pressure: number };
				rain?: { "3h": number };
				wind: { speed: number; deg: number };
				weather: Array<{ main: string }>;
			}>;
		};

		for (const item of weatherData.list) {
			const date = new Date(item.dt * 1000).toISOString().split("T")[0];
			if (!groupedByDay.has(date)) {
				groupedByDay.set(date, []);
			}
			groupedByDay.get(date)?.push(item);
		}

		for (const [date, items] of groupedByDay) {
			if (dailyForecasts.length >= days) break;

			const typedItems = items as Array<{
				main: { temp: number; humidity: number; pressure: number };
				rain?: { "3h": number };
				wind: { speed: number; deg: number };
				weather: Array<{ main: string }>;
			}>;

			const temps = typedItems.map((item) => item.main.temp);
			const humidity =
				typedItems.reduce((sum, item) => sum + item.main.humidity, 0) /
				typedItems.length;
			const precipitation = typedItems.reduce(
				(sum, item) => sum + (item.rain?.["3h"] || 0),
				0,
			);
			const windSpeed =
				typedItems.reduce((sum, item) => sum + item.wind.speed, 0) /
				typedItems.length;
			const windDirection =
				typedItems.reduce((sum, item) => sum + item.wind.deg, 0) /
				typedItems.length;
			const pressure =
				typedItems.reduce((sum, item) => sum + item.main.pressure, 0) /
				typedItems.length;

			dailyForecasts.push({
				date,
				temperature: {
					min: Math.min(...temps),
					max: Math.max(...temps),
					average: temps.reduce((sum, temp) => sum + temp, 0) / temps.length,
				},
				humidity: Math.round(humidity),
				precipitation: {
					probability: typedItems.some((item) => (item.rain?.["3h"] || 0) > 0)
						? 80
						: 20, // Simplified
					amount: precipitation,
				},
				windSpeed: Math.round(windSpeed * 10) / 10,
				windDirection: Math.round(windDirection),
				condition:
					typedItems[Math.floor(typedItems.length / 2)].weather[0].main, // Use middle forecast
				visibility: 10000, // Default visibility
				uvIndex: 5, // Default UV index
				pressure: Math.round(pressure),
			});
		}

		return dailyForecasts;
	}

	async getCurrentWeather(
		latitude: number,
		longitude: number,
	): Promise<WeatherForecastDto> {
		const response = await this.fetcher.fetch(
			`https://api.openweathermap.org/data/2.5/weather?lat=${latitude}&lon=${longitude}&appid=${this.apiKey}&units=metric`,
		);

		if (!response.ok) {
			throw new Error(`Weather API error: ${response.statusText}`);
		}

		const data: unknown = await response.json();

		// Type guard for current weather data
		if (
			!data ||
			typeof data !== "object" ||
			!("main" in data) ||
			!("weather" in data) ||
			!Array.isArray((data as { weather: unknown[] }).weather)
		) {
			throw new Error("Invalid response format from current weather API");
		}

		const weatherData = data as {
			main: {
				temp: number;
				temp_min: number;
				temp_max: number;
				humidity: number;
				pressure: number;
			};
			wind: { speed: number; deg?: number };
			weather: Array<{ main: string }>;
			rain?: { "1h": number };
			visibility?: number;
		};

		return {
			date: new Date().toISOString().split("T")[0],
			temperature: {
				min: weatherData.main.temp_min,
				max: weatherData.main.temp_max,
				average: weatherData.main.temp,
			},
			humidity: weatherData.main.humidity,
			precipitation: {
				probability: weatherData.rain ? 80 : 20,
				amount: weatherData.rain?.["1h"] || 0,
			},
			windSpeed: weatherData.wind.speed,
			windDirection: weatherData.wind.deg || 0,
			condition: weatherData.weather[0].main,
			visibility: weatherData.visibility || 10000,
			uvIndex: 5, // Not available in current weather, would need UV index API
			pressure: weatherData.main.pressure,
		};
	}
}

export class WeatherGovService implements WeatherApiService {
	constructor(private fetcher: Fetcher) { }

	async getForecast(
		latitude: number,
		longitude: number,
		days: number,
	): Promise<WeatherForecastDto[]> {
		// Get grid point first
		const pointResponse = await this.fetcher.fetch(
			`https://api.weather.gov/points/${latitude},${longitude}`,
		);

		if (!pointResponse.ok) {
			throw new Error(`Weather.gov API error: ${pointResponse.statusText}`);
		}

		const pointData = (await pointResponse.json()) as {
			properties: { forecast: string };
		};
		const forecastUrl = pointData.properties.forecast;

		// Get forecast
		const forecastResponse = await this.fetcher.fetch(forecastUrl);

		if (!forecastResponse.ok) {
			throw new Error(
				`Weather.gov forecast error: ${forecastResponse.statusText}`,
			);
		}

		const forecastData = (await forecastResponse.json()) as {
			properties: {
				periods: Array<{
					startTime: string;
					temperature: number;
					probabilityOfPrecipitation?: { value: number };
					windSpeed: string;
					windDirection: string;
					shortForecast: string;
				}>;
			};
		};
		const periods = forecastData.properties.periods.slice(0, days * 2); // Day and night periods

		const dailyForecasts: WeatherForecastDto[] = [];

		for (let i = 0; i < periods.length; i += 2) {
			const dayPeriod = periods[i];
			const nightPeriod = periods[i + 1];

			if (!dayPeriod) break;

			dailyForecasts.push({
				date: new Date(dayPeriod.startTime).toISOString().split("T")[0],
				temperature: {
					min: nightPeriod?.temperature || dayPeriod.temperature - 10,
					max: dayPeriod.temperature,
					average:
						(dayPeriod.temperature +
							(nightPeriod?.temperature || dayPeriod.temperature - 10)) /
						2,
				},
				humidity: 50, // Not provided by weather.gov, default value
				precipitation: {
					probability: dayPeriod.probabilityOfPrecipitation?.value || 0,
					amount: 0, // Not provided in basic forecast
				},
				windSpeed: Number.parseInt(dayPeriod.windSpeed.split(" ")[0]) || 0,
				windDirection: this.windDirectionToAngle(dayPeriod.windDirection),
				condition: dayPeriod.shortForecast,
				visibility: 10000,
				uvIndex: 5,
				pressure: 1013, // Default pressure
			});
		}

		return dailyForecasts;
	}

	async getCurrentWeather(
		latitude: number,
		longitude: number,
	): Promise<WeatherForecastDto> {
		const forecasts = await this.getForecast(latitude, longitude, 1);
		return forecasts[0];
	}

	private windDirectionToAngle(direction: string): number {
		const directions: { [key: string]: number } = {
			N: 0,
			NNE: 22.5,
			NE: 45,
			ENE: 67.5,
			E: 90,
			ESE: 112.5,
			SE: 135,
			SSE: 157.5,
			S: 180,
			SSW: 202.5,
			SW: 225,
			WSW: 247.5,
			W: 270,
			WNW: 292.5,
			NW: 315,
			NNW: 337.5,
		};
		return directions[direction] || 0;
	}
}
