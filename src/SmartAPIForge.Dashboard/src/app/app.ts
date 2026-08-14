import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { API_BASE_URL } from './api-config';
import { StatusService } from './status.service';

type StatusTone = 'good' | 'warning' | 'critical';

@Component({
  selector: 'app-root',
  imports: [DatePipe],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit, OnDestroy {
  protected readonly statusService = inject(StatusService);
  protected readonly apiBaseUrl = API_BASE_URL;

  private refreshHandle?: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    this.statusService.refresh();
    this.refreshHandle = setInterval(() => this.statusService.refresh(), 10_000);
  }

  ngOnDestroy(): void {
    if (this.refreshHandle !== undefined) {
      clearInterval(this.refreshHandle);
    }
  }

  protected statusTone(status: string): StatusTone {
    if (status === 'Healthy') return 'good';
    if (status === 'Degraded') return 'warning';
    return 'critical';
  }

  protected statusIcon(status: string): string {
    if (status === 'Healthy') return '●';
    if (status === 'Degraded') return '▲';
    return '✕';
  }

  /** .NET TimeSpan serializes as "[d.]hh:mm:ss[.fffffff]" — format it for humans. */
  protected formatUptime(raw: string | undefined): string {
    if (!raw) return '—';

    const match = raw.match(/^(?:(\d+)\.)?(\d{2}):(\d{2}):(\d{2})/);
    if (!match) return raw;

    const [, days, hours, minutes, seconds] = match;
    const parts: string[] = [];
    if (days && Number(days) > 0) parts.push(`${days}d`);
    if (Number(hours) > 0 || parts.length > 0) parts.push(`${Number(hours)}h`);
    if (Number(minutes) > 0 || parts.length > 0) parts.push(`${Number(minutes)}m`);
    parts.push(`${Number(seconds)}s`);

    return parts.join(' ');
  }
}
