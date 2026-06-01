import { cn, formatDate, formatDateTime, truncate } from '../utils';

// ── cn ────────────────────────────────────────────────────────────────────────

describe('cn (class name merger)', () => {
  it('returns a single class unchanged', () => {
    expect(cn('text-red-500')).toBe('text-red-500');
  });

  it('merges multiple classes into one string', () => {
    const result = cn('flex', 'items-center', 'gap-2');
    expect(result).toBe('flex items-center gap-2');
  });

  it('resolves Tailwind conflicts — last wins', () => {
    // twMerge removes the first conflicting utility
    const result = cn('text-red-500', 'text-blue-500');
    expect(result).toBe('text-blue-500');
    expect(result).not.toContain('text-red-500');
  });

  it('ignores falsy values (undefined, false, null)', () => {
    const result = cn('base', undefined, false, null, 'extra');
    expect(result).toBe('base extra');
  });

  it('handles conditional objects', () => {
    const isActive = true;
    const result = cn('base', { 'text-primary-600': isActive, 'text-gray-400': !isActive });
    expect(result).toContain('text-primary-600');
    expect(result).not.toContain('text-gray-400');
  });

  it('returns an empty string when given no truthy arguments', () => {
    expect(cn(false, undefined, null)).toBe('');
  });
});

// ── formatDate ────────────────────────────────────────────────────────────────

describe('formatDate', () => {
  it('formats a Date object into a readable string', () => {
    const result = formatDate(new Date('2024-06-15T00:00:00Z'));
    // Should contain the year
    expect(result).toContain('2024');
  });

  it('formats an ISO date string', () => {
    // Use noon local time to avoid UTC midnight crossing a day boundary in any timezone
    const result = formatDate('2024-06-15T12:00:00');
    expect(result).toContain('2024');
    expect(result).toContain('15');
  });

  it('includes the day number in the output', () => {
    // Noon avoids timezone-induced day-shift (midnight UTC → prev day in UTC-N)
    const result = formatDate('2024-03-20T12:00:00', 'en-US');
    expect(result).toContain('20');
  });

  it('returns a non-empty string', () => {
    expect(formatDate('2024-06-15')).not.toBe('');
  });

  it('produces a different string for different dates', () => {
    const d1 = formatDate('2023-01-01');
    const d2 = formatDate('2024-12-31');
    expect(d1).not.toBe(d2);
  });
});

// ── formatDateTime ────────────────────────────────────────────────────────────

describe('formatDateTime', () => {
  it('formats a Date object and includes the year', () => {
    const result = formatDateTime(new Date('2024-06-15T14:30:00Z'));
    expect(result).toContain('2024');
  });

  it('includes time components (AM/PM)', () => {
    const result = formatDateTime(new Date('2024-06-15T14:30:00'), 'en-US');
    // en-US with hour: '2-digit' formats as AM/PM
    expect(result).toMatch(/AM|PM/);
  });

  it('formats an ISO string as well as a Date object', () => {
    const fromString = formatDateTime('2024-06-15T14:30:00Z');
    expect(typeof fromString).toBe('string');
    expect(fromString.length).toBeGreaterThan(0);
  });
});

// ── truncate ──────────────────────────────────────────────────────────────────

describe('truncate', () => {
  it('returns the text unchanged when within maxLength', () => {
    expect(truncate('Hello', 10)).toBe('Hello');
  });

  it('returns the text unchanged when exactly at maxLength', () => {
    expect(truncate('Hello', 5)).toBe('Hello');
  });

  it('truncates and appends "..." when text exceeds maxLength', () => {
    expect(truncate('Hello world', 5)).toBe('Hello...');
  });

  it('truncates to exactly maxLength characters before the ellipsis', () => {
    const result = truncate('abcdefghij', 3);
    expect(result).toBe('abc...');
  });

  it('returns an empty string unchanged', () => {
    expect(truncate('', 10)).toBe('');
  });

  it('handles a maxLength of 0', () => {
    expect(truncate('Hello', 0)).toBe('...');
  });

  it('returns a single long word truncated correctly', () => {
    const long = 'a'.repeat(100);
    const result = truncate(long, 10);
    expect(result).toBe('a'.repeat(10) + '...');
  });
});
