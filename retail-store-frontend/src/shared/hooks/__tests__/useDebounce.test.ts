import { renderHook, act } from '@/test/test-utils';
import { useDebounce } from '../index';

beforeEach(() => vi.useFakeTimers());
afterEach(() => vi.useRealTimers());

describe('useDebounce', () => {
  it('returns the initial value immediately without waiting', () => {
    const { result } = renderHook(() => useDebounce('initial', 300));

    expect(result.current).toBe('initial');
  });

  it('still returns the old value before the delay expires', () => {
    const { result, rerender } = renderHook(
      ({ value }) => useDebounce(value, 300),
      { initialProps: { value: 'first' } }
    );

    rerender({ value: 'second' });
    vi.advanceTimersByTime(299);

    expect(result.current).toBe('first');
  });

  it('returns the new value once the delay has passed', () => {
    const { result, rerender } = renderHook(
      ({ value }) => useDebounce(value, 300),
      { initialProps: { value: 'first' } }
    );

    rerender({ value: 'second' });
    act(() => { vi.advanceTimersByTime(300); });

    expect(result.current).toBe('second');
  });

  it('debounces rapid changes — only the last value is emitted', () => {
    const { result, rerender } = renderHook(
      ({ value }) => useDebounce(value, 300),
      { initialProps: { value: 'a' } }
    );

    rerender({ value: 'b' });
    vi.advanceTimersByTime(100);
    rerender({ value: 'c' });
    vi.advanceTimersByTime(100);
    rerender({ value: 'd' });
    act(() => { vi.advanceTimersByTime(300); });

    // Only 'd' (the last value) should have been committed
    expect(result.current).toBe('d');
  });

  it('works with non-string values (numbers)', () => {
    const { result, rerender } = renderHook(
      ({ value }) => useDebounce(value, 200),
      { initialProps: { value: 0 } }
    );

    rerender({ value: 42 });
    act(() => { vi.advanceTimersByTime(200); });

    expect(result.current).toBe(42);
  });

  it('respects a custom delay', () => {
    const { result, rerender } = renderHook(
      ({ value }) => useDebounce(value, 1000),
      { initialProps: { value: 'slow' } }
    );

    rerender({ value: 'fast' });
    act(() => { vi.advanceTimersByTime(999); });
    expect(result.current).toBe('slow');

    act(() => { vi.advanceTimersByTime(1); });
    expect(result.current).toBe('fast');
  });
});
