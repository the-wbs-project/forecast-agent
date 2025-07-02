import { KVProjectDataService, KVService, KVTaskDataService, KVWeatherDataService } from '../data-services';
import { TokenPayload } from '../dto';
import { AuthService, WeatherApiService } from '../http-services';
import { WeatherGuardApiService } from '../api-services/external-api.service';
import type { DataDogService, Logger } from '../services/logging';
import type { Fetcher } from '../services/fetcher.service';

export type Variables = {
	kvService: KVService,
	authService: AuthService,
	projectService: KVProjectDataService,
	taskService: KVTaskDataService,
	weatherService: KVWeatherDataService,
	weatherApiService: WeatherApiService,
	weatherGuardApiService?: WeatherGuardApiService,
	user: TokenPayload | null,
	datadog: DataDogService,
	logger: Logger,
	fetcher: Fetcher
};
