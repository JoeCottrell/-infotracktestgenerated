import { useState } from 'react';
import SearchPanel from './components/SearchPanel';
import LocationManager from './components/LocationManager';
import ReportView from './components/ReportView';
import HistoryPanel from './components/HistoryPanel';
import type { ReportDto, SearchResultDto } from './types';
import { historyApi } from './services/api';
import './App.css';

type Tab = 'search' | 'locations' | 'history';

export default function App() {
  const [tab, setTab] = useState<Tab>('search');
  const [currentReport, setCurrentReport] = useState<ReportDto | null>(null);
  const [loadingReport, setLoadingReport] = useState(false);

  const handleSearchResult = async (result: SearchResultDto) => {
    try {
      setLoadingReport(true);
      const report = await historyApi.getReport(result.searchRecordId);
      setCurrentReport(report);
      setTab('search');
    } catch {
      // still show something
      setCurrentReport(null);
    } finally {
      setLoadingReport(false);
    }
  };

  return (
    <div className="app">
      <header className="app-header">
        <div className="header-inner">
          <div className="brand">
            <span className="brand-logo">⚖</span>
            <span className="brand-name">InfoTrack</span>
            <span className="brand-sub">Solicitor Search</span>
          </div>
          <nav className="tab-nav">
            {(['search', 'locations', 'history'] as Tab[]).map(t => (
              <button
                key={t}
                className={`tab-btn ${tab === t ? 'active' : ''}`}
                onClick={() => setTab(t)}
              >
                {t.charAt(0).toUpperCase() + t.slice(1)}
              </button>
            ))}
          </nav>
        </div>
      </header>

      <main className="app-main">
        {tab === 'search' && (
          <>
            <SearchPanel onResult={handleSearchResult} />
            {loadingReport && (
              <div className="card"><p className="muted">Building report…</p></div>
            )}
            {currentReport && !loadingReport && (
              <>
                <div className="section-header">
                  <h2>Latest Results — {new Date(currentReport.searchedAt).toLocaleString()}</h2>
                </div>
                <ReportView report={currentReport} />
              </>
            )}
          </>
        )}

        {tab === 'locations' && <LocationManager />}

        {tab === 'history' && <HistoryPanel />}
      </main>

      <footer className="app-footer">
        <p>InfoTrack · Data sourced from solicitors.com</p>
      </footer>
    </div>
  );
}
