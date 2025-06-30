import { KVProjectDataService, KVService, KVTaskDataService, KVWeatherDataService } from '../data-services';
import { TokenPayload } from '../dto';
import { AuthService, WeatherApiService } from '../http-services';

export type Variables = {
	kvService: KVService,
	authService: AuthService,
	projectService: KVProjectDataService,
	taskService: KVTaskDataService,
	weatherService: KVWeatherDataService,
	weatherApiService: WeatherApiService,
	user: TokenPayload | null
};
