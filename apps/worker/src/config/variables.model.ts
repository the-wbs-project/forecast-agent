import type { TokenPayload } from "../dto";
import type { WeatherGuardApiService } from "../services/api-services/external-api.service";
import type {
	KVProjectDataService,
	KVService,
	KVTaskDataService,
	KVWeatherDataService,
} from "../services/data-services";
import type { Fetcher } from "../services/fetcher.service";
import type {
	AuthHttpServiceType,
	WeatherHttpServiceType,
} from "../services/http-services";
import type { DataDogService, Logger } from "../services/logging";

export type Variables = {
	kvService: KVService;
	authService: AuthHttpServiceType;
	projectService: KVProjectDataService;
	taskService: KVTaskDataService;
	weatherService: KVWeatherDataService;
	weatherApiService: WeatherHttpServiceType;
	weatherGuardApiService?: WeatherGuardApiService;
	user: TokenPayload | null;
	datadog: DataDogService;
	logger: Logger;
	fetcher: Fetcher;
};
