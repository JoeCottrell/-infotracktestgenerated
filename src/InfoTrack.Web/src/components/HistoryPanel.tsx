import { useState, useEffect } from 'react';
import { historyApi } from '../services/api';
import type { SearchHistoryItem, ReportDto } from '../types';
import ReportView from './ReportView';

export default function HistoryPanel() {
  const [history, setHistory]   = useState<SearchHistoryItem[]>([]);
  const [loading, setLoading]   = useState(true);
  const [report, setReport]     = useState<ReportDto | null>(null);
  const [loadingId, setLoadingId] = useState<number | null>(null);
  const [error, setError]       = useState<string | null>(null);

  useEffect(() => {
    historyApi.getAll()
      .then(setHistory)
      .catch(e => setError(e.message))
      .finally(() => setLoading(false));
  }, []);

  const openReport = async (id: number) => {
    if (loadingId === id) return;
    try {
      setLoadingId(id);
      setError(null);
      const r = await historyApi.getReport(id);
      setReport(r);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load report');
    } finally {
      setLoadingId(null);
    }
  };

  if (loading) return <div className="card"><p className="muted">Loading history…</p></div>;

  return (
    <div>
      <div className="card">
        <h2>Search History</h2>
        {error && <p className="error">{error}</p>}
        {history.length === 0 ? (
          <p className="muted">No searches yet. Run a search to see results here.</p>
        ) : (
          <table className="locations-table">
            <thead>
              <tr>
                <th>Date / Time</th>
                <th>Locations</th>
                <th>Results</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {history.map(item => (
                <tr key={item.id}>
                  <td>{new Date(item.searchedAt).toLocaleString()}</td>
                  <td>{item.locations.join(', ')}</td>
                  <td>{item.totalResults}</td>
                  <td>
                    <button
                      className="btn-sm"
                      onClick={() => openReport(item.id)}
                      disabled={loadingId === item.id}
                    >
                      {loadingId === item.id ? 'Loading…' : 'View Report'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {report && (
        <div>
          <div className="section-header">
            <h2>Report — {new Date(report.searchedAt).toLocaleString()}</h2>
            <button className="btn-sm" onClick={() => setReport(null)}>Close</button>
          </div>
          <ReportView report={report} />
        </div>
      )}
    </div>
  );
}
