import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { API_BASE_URL } from './api-config';

export interface SystemStatus {
  status: string;
  environment: string;
  serverTimeUtc: string;
  databaseProvider: string;
  uptime: string;
  version: string;
}

@Injectable({ providedIn: 'root' })
export class StatusService {
  private readonly http = inject(HttpClient);

  readonly status = signal<SystemStatus | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  refresh(): void {
    this.loading.set(true);
    this.http.get<SystemStatus>(`${API_BASE_URL}/system/status`).subscribe({
      next: (result) => {
        this.status.set(result);
        this.error.set(null);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(`Could not reach the API at ${API_BASE_URL}.`);
        this.loading.set(false);
      },
    });
  }
}
