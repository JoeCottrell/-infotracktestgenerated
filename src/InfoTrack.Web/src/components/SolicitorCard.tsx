import type { Solicitor } from '../types';

interface Props {
  solicitor: Solicitor;
}

function StarRating({ value }: { value: number }) {
  const full  = Math.floor(value);
  const half  = value - full >= 0.5;
  const empty = 5 - full - (half ? 1 : 0);
  return (
    <span className="stars" title={`${value} / 5`}>
      {'★'.repeat(full)}
      {half ? '½' : ''}
      {'☆'.repeat(empty)}
    </span>
  );
}

export default function SolicitorCard({ solicitor }: Props) {
  return (
    <div className="solicitor-card">
      <div className="solicitor-header">
        <h3>{solicitor.name}</h3>
        <span className="location-tag">{solicitor.location}</span>
      </div>

      {solicitor.rating !== undefined && solicitor.rating !== null && (
        <div className="rating-row">
          <StarRating value={solicitor.rating} />
          <span className="rating-label">{solicitor.rating.toFixed(1)}</span>
          {solicitor.reviewCount !== undefined && solicitor.reviewCount !== null && (
            <span className="muted">({solicitor.reviewCount} reviews)</span>
          )}
        </div>
      )}

      {solicitor.description && (
        <p className="solicitor-desc">{solicitor.description}</p>
      )}

      <div className="contact-details">
        {solicitor.address && (
          <div className="contact-item">
            <span className="contact-icon">📍</span>
            <span>{solicitor.address}</span>
          </div>
        )}
        {solicitor.phone && (
          <div className="contact-item">
            <span className="contact-icon">📞</span>
            <a href={`tel:${solicitor.phone}`}>{solicitor.phone}</a>
          </div>
        )}
        {solicitor.email && (
          <div className="contact-item">
            <span className="contact-icon">✉️</span>
            <a href={`mailto:${solicitor.email}`}>{solicitor.email}</a>
          </div>
        )}
        {solicitor.website && (
          <div className="contact-item">
            <span className="contact-icon">🌐</span>
            <a href={solicitor.website} target="_blank" rel="noreferrer noopener">
              Visit website
            </a>
          </div>
        )}
      </div>
    </div>
  );
}
