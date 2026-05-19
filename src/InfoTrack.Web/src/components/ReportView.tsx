import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer, Cell,
} from 'recharts';
import type { ReportDto } from '../types';
import SolicitorCard from './SolicitorCard';

interface Props {
  report: ReportDto;
}

const COLOURS = [
  '#2563eb', '#7c3aed', '#db2777', '#ea580c',
  '#16a34a', '#0891b2', '#ca8a04', '#dc2626',
];

export default function ReportView({ report }: Props) {
  const { locationSummaries, topRated, allSolicitors } = report;

  const barData = locationSummaries.map(l => ({
    name: l.location,
    count: l.count,
    avgRating: l.averageRating ? parseFloat(l.averageRating.toFixed(2)) : 0,
  }));

  return (
    <div className="report">
      {/* ── Summary strip ───────────────────────────────────── */}
      <div className="stat-grid">
        <div className="stat-card">
          <div className="stat-value">{report.totalResults}</div>
          <div className="stat-label">Total Solicitors</div>
        </div>
        <div className="stat-card">
          <div className="stat-value">{report.locations.length}</div>
          <div className="stat-label">Locations Searched</div>
        </div>
        <div className="stat-card">
          <div className="stat-value">
            {locationSummaries.length > 0
              ? (locationSummaries.reduce((s, l) => s + (l.averageRating ?? 0), 0) /
                 locationSummaries.filter(l => l.averageRating).length || 0
                ).toFixed(1)
              : '—'}
          </div>
          <div className="stat-label">Avg Rating</div>
        </div>
        <div className="stat-card">
          <div className="stat-value">
            {locationSummaries.reduce((s, l) => s + l.totalReviews, 0).toLocaleString()}
          </div>
          <div className="stat-label">Total Reviews</div>
        </div>
      </div>

      {/* ── Results by location ─────────────────────────────── */}
      {barData.length > 0 && (
        <div className="card">
          <h3>Solicitors by Location</h3>
          <ResponsiveContainer width="100%" height={280}>
            <BarChart data={barData} margin={{ top: 8, right: 16, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
              <XAxis dataKey="name" tick={{ fontSize: 12 }} />
              <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
              <Tooltip />
              <Bar dataKey="count" name="Solicitors" radius={[4, 4, 0, 0]}>
                {barData.map((_, i) => (
                  <Cell key={i} fill={COLOURS[i % COLOURS.length]} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* ── Average rating by location ──────────────────────── */}
      {barData.some(d => d.avgRating > 0) && (
        <div className="card">
          <h3>Average Rating by Location</h3>
          <ResponsiveContainer width="100%" height={250}>
            <BarChart data={barData.filter(d => d.avgRating > 0)} margin={{ top: 8, right: 16, left: 0, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
              <XAxis dataKey="name" tick={{ fontSize: 12 }} />
              <YAxis domain={[0, 5]} tick={{ fontSize: 12 }} />
              <Tooltip formatter={(v: number) => v.toFixed(2)} />
              <Bar dataKey="avgRating" name="Avg Rating" radius={[4, 4, 0, 0]}>
                {barData.map((_, i) => (
                  <Cell key={i} fill={COLOURS[i % COLOURS.length]} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* ── Location breakdown table ────────────────────────── */}
      <div className="card">
        <h3>Location Breakdown</h3>
        <table className="locations-table">
          <thead>
            <tr>
              <th>Location</th>
              <th>Count</th>
              <th>Avg Rating</th>
              <th>Total Reviews</th>
            </tr>
          </thead>
          <tbody>
            {locationSummaries.map(l => (
              <tr key={l.location}>
                <td>{l.location}</td>
                <td>{l.count}</td>
                <td>{l.averageRating ? l.averageRating.toFixed(2) : '—'}</td>
                <td>{l.totalReviews.toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* ── Top rated ───────────────────────────────────────── */}
      {topRated.length > 0 && (
        <div className="card">
          <h3>Top Rated Solicitors</h3>
          <div className="card-grid">
            {topRated.map(s => (
              <SolicitorCard key={s.id} solicitor={s} />
            ))}
          </div>
        </div>
      )}

      {/* ── Full results ────────────────────────────────────── */}
      <div className="card">
        <h3>All Results ({allSolicitors.length})</h3>
        <div className="card-grid">
          {allSolicitors.map(s => (
            <SolicitorCard key={s.id} solicitor={s} />
          ))}
        </div>
      </div>
    </div>
  );
}
