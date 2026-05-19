import { useState, useEffect, useCallback } from 'react';
import { locationsApi } from '../services/api';
import type { Location } from '../types';

export function useLocations() {
  const [locations, setLocations] = useState<Location[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      setLocations(await locationsApi.getAll());
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load locations');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const add = async (name: string) => {
    const created = await locationsApi.create(name);
    setLocations(prev => [...prev, created].sort((a, b) => a.name.localeCompare(b.name)));
  };

  const toggle = async (id: number) => {
    const updated = await locationsApi.toggle(id);
    setLocations(prev => prev.map(l => l.id === id ? updated : l));
  };

  const remove = async (id: number) => {
    await locationsApi.remove(id);
    setLocations(prev => prev.filter(l => l.id !== id));
  };

  return { locations, loading, error, reload: load, add, toggle, remove };
}
