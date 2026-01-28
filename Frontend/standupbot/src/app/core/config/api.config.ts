import { InjectionToken } from '@angular/core';

/**
 * API configuration interface.
 */
export interface ApiConfig {
  baseUrl: string;
  timeout: number;
}

/**
 * Injection token for API configuration.
 */
export const API_CONFIG = new InjectionToken<ApiConfig>('API_CONFIG');

/**
 * Default API configuration.
 */
export const apiConfig: ApiConfig = {
  baseUrl: 'http://localhost:5091/api',
  timeout: 30000
};
