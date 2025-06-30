import { WeatherForecastDto } from '../../dto';

export interface WeatherApiService {
  getForecast(latitude: number, longitude: number, days: number): Promise<WeatherForecastDto[]>;
  getCurrentWeather(latitude: number, longitude: number): Promise<WeatherForecastDto>;
}

export class OpenWeatherMapService implements WeatherApiService {
  constructor(private apiKey: string) { }

  async getForecast(latitude: number, longitude: number, days: number): Promise<WeatherForecastDto[]> {
    const response = await fetch(
      `https://api.openweathermap.org/data/2.5/forecast?lat=${latitude}&lon=${longitude}&appid=${this.apiKey}&units=metric&cnt=${days * 8}` // 8 forecasts per day (3-hour intervals)
    );

    if (!response.ok) {
      throw new Error(`Weather API error: ${response.statusText}`);
    }

    const data: any = await response.json();

    // Group by day and aggregate
    const dailyForecasts: WeatherForecastDto[] = [];
    const groupedByDay = new Map<string, any[]>();

    data.list.forEach((item: any) => {
      const date = new Date(item.dt * 1000).toISOString().split('T')[0];
      if (!groupedByDay.has(date)) {
        groupedByDay.set(date, []);
      }
      groupedByDay.get(date)!.push(item);
    });

    for (const [date, items] of groupedByDay) {
      if (dailyForecasts.length >= days) break;

      const temps = items.map(item => item.main.temp);
      const humidity = items.reduce((sum, item) => sum + item.main.humidity, 0) / items.length;
      const precipitation = items.reduce((sum, item) => sum + (item.rain?.['3h'] || 0), 0);
      const windSpeed = items.reduce((sum, item) => sum + item.wind.speed, 0) / items.length;
      const windDirection = items.reduce((sum, item) => sum + item.wind.deg, 0) / items.length;
      const pressure = items.reduce((sum, item) => sum + item.main.pressure, 0) / items.length;

      dailyForecasts.push({
        date,
        temperature: {
          min: Math.min(...temps),
          max: Math.max(...temps),
          average: temps.reduce((sum, temp) => sum + temp, 0) / temps.length
        },
        humidity: Math.round(humidity),
        precipitation: {
          probability: items.some(item => (item.rain?.['3h'] || 0) > 0) ? 80 : 20, // Simplified
          amount: precipitation
        },
        windSpeed: Math.round(windSpeed * 10) / 10,
        windDirection: Math.round(windDirection),
        condition: items[Math.floor(items.length / 2)].weather[0].main, // Use middle forecast
        visibility: 10000, // Default visibility
        uvIndex: 5, // Default UV index
        pressure: Math.round(pressure)
      });
    }

    return dailyForecasts;
  }

  async getCurrentWeather(latitude: number, longitude: number): Promise<WeatherForecastDto> {
    const response = await fetch(
      `https://api.openweathermap.org/data/2.5/weather?lat=${latitude}&lon=${longitude}&appid=${this.apiKey}&units=metric`
    );

    if (!response.ok) {
      throw new Error(`Weather API error: ${response.statusText}`);
    }

    const data: any = await response.json();

    return {
      date: new Date().toISOString().split('T')[0],
      temperature: {
        min: data.main.temp_min,
        max: data.main.temp_max,
        average: data.main.temp
      },
      humidity: data.main.humidity,
      precipitation: {
        probability: data.rain ? 80 : 20,
        amount: data.rain?.['1h'] || 0
      },
      windSpeed: data.wind.speed,
      windDirection: data.wind.deg || 0,
      condition: data.weather[0].main,
      visibility: data.visibility || 10000,
      uvIndex: 5, // Not available in current weather, would need UV index API
      pressure: data.main.pressure
    };
  }
}

export class WeatherGovService implements WeatherApiService {
  async getForecast(latitude: number, longitude: number, days: number): Promise<WeatherForecastDto[]> {
    // Get grid point first
    const pointResponse = await fetch(
      `https://api.weather.gov/points/${latitude},${longitude}`
    );

    if (!pointResponse.ok) {
      throw new Error(`Weather.gov API error: ${pointResponse.statusText}`);
    }

    const pointData: any = await pointResponse.json();
    const forecastUrl = pointData.properties.forecast;

    // Get forecast
    const forecastResponse = await fetch(forecastUrl);

    if (!forecastResponse.ok) {
      throw new Error(`Weather.gov forecast error: ${forecastResponse.statusText}`);
    }

    const forecastData: any = await forecastResponse.json();
    const periods = forecastData.properties.periods.slice(0, days * 2); // Day and night periods

    const dailyForecasts: WeatherForecastDto[] = [];

    for (let i = 0; i < periods.length; i += 2) {
      const dayPeriod = periods[i];
      const nightPeriod = periods[i + 1];

      if (!dayPeriod) break;

      dailyForecasts.push({
        date: new Date(dayPeriod.startTime).toISOString().split('T')[0],
        temperature: {
          min: nightPeriod?.temperature || dayPeriod.temperature - 10,
          max: dayPeriod.temperature,
          average: (dayPeriod.temperature + (nightPeriod?.temperature || dayPeriod.temperature - 10)) / 2
        },
        humidity: 50, // Not provided by weather.gov, default value
        precipitation: {
          probability: dayPeriod.probabilityOfPrecipitation?.value || 0,
          amount: 0 // Not provided in basic forecast
        },
        windSpeed: parseInt(dayPeriod.windSpeed.split(' ')[0]) || 0,
        windDirection: this.windDirectionToAngle(dayPeriod.windDirection),
        condition: dayPeriod.shortForecast,
        visibility: 10000,
        uvIndex: 5,
        pressure: 1013 // Default pressure
      });
    }

    return dailyForecasts;
  }

  async getCurrentWeather(latitude: number, longitude: number): Promise<WeatherForecastDto> {
    const forecasts = await this.getForecast(latitude, longitude, 1);
    return forecasts[0];
  }

  private windDirectionToAngle(direction: string): number {
    const directions: { [key: string]: number } = {
      'N': 0, 'NNE': 22.5, 'NE': 45, 'ENE': 67.5,
      'E': 90, 'ESE': 112.5, 'SE': 135, 'SSE': 157.5,
      'S': 180, 'SSW': 202.5, 'SW': 225, 'WSW': 247.5,
      'W': 270, 'WNW': 292.5, 'NW': 315, 'NNW': 337.5
    };
    return directions[direction] || 0;
  }
}