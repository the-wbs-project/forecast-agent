import type { TokenPayload } from "../dto";
import type {
	DataDogService,
	Fetcher,
	IAuthService,
	KVProjectDataService,
	KVService,
	KVTaskDataService,
	KVWeatherDataService,
	Logger,
	WeatherApiService,
	WeatherGuardApiService,
} from "../services";

export type Variables = {
	kvService: KVService;
	authService: IAuthService;
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
