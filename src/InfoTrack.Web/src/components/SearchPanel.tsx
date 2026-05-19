import { useState } from 'react';
import { searchApi } from '../services/api';
import type { SearchResultDto } from '../types';

interface Props {
  onResult: (result: SearchResultDto) => void;
}

export default function SearchPanel({ onResult }: Props) {
  const [loading, setLoading] = useState(false);
  const [error, setError]     = useState<string | null>(null);

  const runSearch = async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await searchApi.run();
      onResult(result);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Search failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="card search-panel">
      <h2>Conveyancing Search</h2>
      <p className="muted">
        Searches <strong>solicitors.com/conveyancing.html</strong> for all active locations.
      </p>
      {error && <p className="error">{error}</p>}
      <button className="btn-primary" onClick={runSearch} disabled={loading}>
        {loading ? (
          <>
            <span className="spinner" /> Scraping…
          </>
        ) : (
          'Run Search'
        )}
      </button>
      {loading && (
        <p className="muted" style={{ marginTop: '0.5rem' }}>
          This may take a minute — fetching results for each location.
        </p>
      )}
    </div>
  );
}
