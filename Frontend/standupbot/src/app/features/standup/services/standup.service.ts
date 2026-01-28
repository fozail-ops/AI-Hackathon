import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { API_CONFIG } from '../../../core/config/api.config';
import { 
  Standup, 
  StandupSummary, 
  CreateStandupRequest, 
  UpdateStandupRequest,
  SubmissionStatus 
} from '../models/standup.model';

@Injectable({ providedIn: 'root' })
export class StandupService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(API_CONFIG);
  
  private readonly endpoint = 'standups';
  
  // Reactive state
  private readonly _todayStandup = signal<Standup | null>(null);
  private readonly _history = signal<StandupSummary[]>([]);
  private readonly _loading = signal<boolean>(false);
  private readonly _error = signal<string | null>(null);
  private readonly _hasSubmittedToday = signal<boolean>(false);
  
  // Public readonly signals
  readonly todayStandup = this._todayStandup.asReadonly();
  readonly history = this._history.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly hasSubmittedToday = this._hasSubmittedToday.asReadonly();
  
  // Computed
  readonly isEditMode = computed(() => this._todayStandup() !== null);
  
  private get baseUrl(): string {
    return `${this.config.baseUrl}/${this.endpoint}`;
  }
  
  /**
   * Gets today's standup for a user.
   */
  getTodayStandup(userId: number): Observable<Standup> {
    this._loading.set(true);
    this._error.set(null);
    
    return this.http.get<Standup>(`${this.baseUrl}/today/${userId}`).pipe(
      tap(standup => {
        this._todayStandup.set(standup);
        this._hasSubmittedToday.set(true);
        this._loading.set(false);
      }),
      catchError(error => {
        if (error.status === 404) {
          this._todayStandup.set(null);
          this._hasSubmittedToday.set(false);
          this._loading.set(false);
          return throwError(() => error);
        }
        this._error.set(error.message);
        this._loading.set(false);
        return throwError(() => error);
      })
    );
  }
  
  /**
   * Gets standup history for a user.
   */
  getHistory(userId: number, count = 10): Observable<StandupSummary[]> {
    this._loading.set(true);
    
    return this.http.get<StandupSummary[]>(
      `${this.baseUrl}/history/${userId}`,
      { params: { count: count.toString() } }
    ).pipe(
      tap(history => {
        this._history.set(history);
        this._loading.set(false);
      }),
      catchError(error => {
        this._error.set(error.message);
        this._loading.set(false);
        return throwError(() => error);
      })
    );
  }
  
  /**
   * Creates a new standup.
   */
  create(userId: number, request: CreateStandupRequest): Observable<Standup> {
    this._loading.set(true);
    this._error.set(null);
    
    return this.http.post<Standup>(`${this.baseUrl}/${userId}`, request).pipe(
      tap(standup => {
        this._todayStandup.set(standup);
        this._hasSubmittedToday.set(true);
        this._loading.set(false);
      }),
      catchError(error => {
        this._error.set(error.error?.error || error.message);
        this._loading.set(false);
        return throwError(() => error);
      })
    );
  }
  
  /**
   * Updates today's standup.
   */
  update(userId: number, request: UpdateStandupRequest): Observable<Standup> {
    this._loading.set(true);
    this._error.set(null);
    
    return this.http.put<Standup>(`${this.baseUrl}/${userId}`, request).pipe(
      tap(standup => {
        this._todayStandup.set(standup);
        this._loading.set(false);
      }),
      catchError(error => {
        this._error.set(error.error?.error || error.message);
        this._loading.set(false);
        return throwError(() => error);
      })
    );
  }
  
  /**
   * Checks submission status.
   */
  checkSubmissionStatus(userId: number): Observable<SubmissionStatus> {
    return this.http.get<SubmissionStatus>(`${this.baseUrl}/status/${userId}`).pipe(
      tap(status => {
        this._hasSubmittedToday.set(status.hasSubmittedToday);
      })
    );
  }
  
  /**
   * Clears state (for logout).
   */
  clearState(): void {
    this._todayStandup.set(null);
    this._history.set([]);
    this._hasSubmittedToday.set(false);
    this._error.set(null);
  }
}
