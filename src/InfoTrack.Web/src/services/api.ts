import type {
  Location,
  SearchResultDto,
  SearchHistoryItem,
  ReportDto,
} from '../types';

const BASE = '/api';

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { 'Content-Type': 'application/json', ...init?.headers },
    ...init,
  });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(`API ${res.status}: ${text}`);
  }
  return res.json() as Promise<T>;
}

// ─── Locations ────────────────────────────────────────────────────────────

export const locationsApi = {
  getAll: () => request<Location[]>('/locations'),

  create: (name: string) =>
    request<Location>('/locations', {
      method: 'POST',
      body: JSON.stringify({ name }),
    }),

  update: (id: number, name: string, isActive: boolean) =>
    request<Location>(`/locations/${id}`, {
      method: 'PUT',
      body: JSON.stringify({ name, isActive }),
    }),

  remove: (id: number) =>
    request<void>(`/locations/${id}`, { method: 'DELETE' }),

  toggle: (id: number) =>
    request<Location>(`/locations/${id}/toggle`, { method: 'PATCH' }),
};

// ─── Search ───────────────────────────────────────────────────────────────

export const searchApi = {
  run: (locations?: string[]) =>
    request<SearchResultDto>('/solicitors/search', {
      method: 'POST',
      body: JSON.stringify({ locations: locations ?? null }),
    }),

  getResult: (searchRecordId: number) =>
    request<SearchResultDto>(`/solicitors/${searchRecordId}`),
};

// ─── History + Reports ────────────────────────────────────────────────────

export const historyApi = {
  getAll: () => request<SearchHistoryItem[]>('/history'),

  getReport: (searchRecordId: number) =>
    request<ReportDto>(`/history/${searchRecordId}/report`),
};
