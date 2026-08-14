import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from './api-config';
import { App } from './app';

const sampleStatus = {
  status: 'Healthy',
  environment: 'Development',
  serverTimeUtc: new Date().toISOString(),
  databaseProvider: 'Postgres',
  uptime: '00:01:00',
  version: '1.0.0.0',
};

describe('App', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create the app and fetch status on init', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    httpMock.expectOne(`${API_BASE_URL}/system/status`).flush(sampleStatus);

    expect(fixture.componentInstance).toBeTruthy();
    fixture.destroy();
  });

  it('should render the fetched status in a stat tile', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    httpMock.expectOne(`${API_BASE_URL}/system/status`).flush(sampleStatus);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('SmartAPI Forge');
    expect(compiled.textContent).toContain('Healthy');
    fixture.destroy();
  });
});
