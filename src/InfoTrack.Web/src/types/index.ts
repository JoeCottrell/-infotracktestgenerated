export interface Location {
  id: number;
  name: string;
  isActive: boolean;
  createdAt: string;
}

export interface Solicitor {
  id: number;
  name: string;
  location: string;
  address?: string;
  phone?: string;
  email?: string;
  website?: string;
  rating?: number;
  reviewCount?: number;
  description?: string;
  sourceUrl: string;
  scrapedAt: string;
}

export interface SearchResultDto {
  searchRecordId: number;
  searchedAt: string;
  locations: string[];
  totalResults: number;
  solicitors: Solicitor[];
}

export interface SearchHistoryItem {
  id: number;
  searchedAt: string;
  locations: string[];
  totalResults: number;
}

export interface LocationSummary {
  location: string;
  count: number;
  averageRating?: number;
  totalReviews: number;
}

export interface ReportDto {
  searchRecordId: number;
  searchedAt: string;
  locations: string[];
  totalResults: number;
  locationSummaries: LocationSummary[];
  topRated: Solicitor[];
  allSolicitors: Solicitor[];
}
