import type { WeatherGuardApiService } from "../api-services/external-api.service";
import type {
	KVProjectDataService,
	KVService,
	KVTaskDataService,
	KVWeatherDataService,
} from "../data-services";
import type { TokenPayload } from "../dto";
import type { AuthService, WeatherApiService } from "../http-services";
import type { Fetcher } from "../services/fetcher.service";
import type { DataDogService, Logger } from "../services/logging";

export type Variables = {
	kvService: KVService;
	authService: AuthService;
	projectService: KVProjectDataService;
	taskService: KVTaskDataService;
	weatherService: KVWeatherDataService;
	weatherApiService: WeatherApiService;
	weatherGuardApiService?: WeatherGuardApiService;
	user: TokenPayload | null;
	datadog: DataDogService;
	logger: Logger;
	fetcher: Fetcher;
};
