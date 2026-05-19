import { useState } from 'react';
import { useLocations } from '../hooks/useLocations';

export default function LocationManager() {
  const { locations, loading, error, add, toggle, remove } = useLocations();
  const [newName, setNewName] = useState('');
  const [adding, setAdding] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newName.trim()) return;
    try {
      setAdding(true);
      setActionError(null);
      await add(newName.trim());
      setNewName('');
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Failed to add location');
    } finally {
      setAdding(false);
    }
  };

  const handleRemove = async (id: number, name: string) => {
    if (!window.confirm(`Remove "${name}"?`)) return;
    try {
      setActionError(null);
      await remove(id);
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Failed to remove location');
    }
  };

  if (loading) return <div className="card"><p className="muted">Loading locations…</p></div>;
  if (error)   return <div className="card"><p className="error">{error}</p></div>;

  const active   = locations.filter(l => l.isActive);
  const inactive = locations.filter(l => !l.isActive);

  return (
    <div className="card">
      <h2>Manage Locations</h2>
      <p className="muted">
        {active.length} active · {inactive.length} inactive
      </p>

      {actionError && <p className="error">{actionError}</p>}

      <form className="add-form" onSubmit={handleAdd}>
        <input
          type="text"
          placeholder="New location name…"
          value={newName}
          onChange={e => setNewName(e.target.value)}
          disabled={adding}
        />
        <button type="submit" disabled={adding || !newName.trim()}>
          {adding ? 'Adding…' : 'Add'}
        </button>
      </form>

      <table className="locations-table">
        <thead>
          <tr>
            <th>Location</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {locations.map(loc => (
            <tr key={loc.id} className={loc.isActive ? '' : 'inactive-row'}>
              <td>{loc.name}</td>
              <td>
                <span className={`badge ${loc.isActive ? 'badge-active' : 'badge-inactive'}`}>
                  {loc.isActive ? 'Active' : 'Inactive'}
                </span>
              </td>
              <td className="actions">
                <button className="btn-sm" onClick={() => toggle(loc.id)}>
                  {loc.isActive ? 'Disable' : 'Enable'}
                </button>
                <button className="btn-sm btn-danger" onClick={() => handleRemove(loc.id, loc.name)}>
                  Remove
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
