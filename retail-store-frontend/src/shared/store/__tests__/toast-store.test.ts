import { useToastStore } from '../toast-store';

beforeEach(() => {
  vi.useFakeTimers();
  useToastStore.setState({ toasts: [] });
});

afterEach(() => {
  vi.useRealTimers();
});

describe('useToastStore', () => {
  // ── addToast ──────────────────────────────────────────────────────────────

  it('adds a toast to the list', () => {
    useToastStore.getState().addToast({ type: 'success', message: 'Saved!' });

    const { toasts } = useToastStore.getState();
    expect(toasts).toHaveLength(1);
    expect(toasts[0].message).toBe('Saved!');
    expect(toasts[0].type).toBe('success');
  });

  it('assigns a unique id to each toast', () => {
    useToastStore.getState().addToast({ type: 'info', message: 'First' });
    useToastStore.getState().addToast({ type: 'info', message: 'Second' });

    const { toasts } = useToastStore.getState();
    expect(toasts).toHaveLength(2);
    expect(toasts[0].id).toBeDefined();
    expect(toasts[1].id).toBeDefined();
    expect(toasts[0].id).not.toBe(toasts[1].id);
  });

  it('stacks multiple toasts', () => {
    useToastStore.getState().addToast({ type: 'success', message: 'A' });
    useToastStore.getState().addToast({ type: 'error', message: 'B' });
    useToastStore.getState().addToast({ type: 'warning', message: 'C' });

    expect(useToastStore.getState().toasts).toHaveLength(3);
  });

  // ── removeToast ───────────────────────────────────────────────────────────

  it('removes a toast by its id', () => {
    useToastStore.getState().addToast({ type: 'info', message: 'Remove me' });
    const id = useToastStore.getState().toasts[0].id;

    useToastStore.getState().removeToast(id);

    expect(useToastStore.getState().toasts).toHaveLength(0);
  });

  it('only removes the targeted toast when multiple exist', () => {
    useToastStore.getState().addToast({ type: 'info', message: 'Keep' });
    useToastStore.getState().addToast({ type: 'error', message: 'Remove' });
    const removeId = useToastStore.getState().toasts[1].id;

    useToastStore.getState().removeToast(removeId);

    const { toasts } = useToastStore.getState();
    expect(toasts).toHaveLength(1);
    expect(toasts[0].message).toBe('Keep');
  });

  it('is a no-op when the id does not exist', () => {
    useToastStore.getState().addToast({ type: 'success', message: 'Stays' });

    useToastStore.getState().removeToast('non-existent-id');

    expect(useToastStore.getState().toasts).toHaveLength(1);
  });

  // ── auto-remove ───────────────────────────────────────────────────────────

  it('auto-removes the toast after 5 seconds', () => {
    useToastStore.getState().addToast({ type: 'success', message: 'Auto-gone' });
    expect(useToastStore.getState().toasts).toHaveLength(1);

    vi.advanceTimersByTime(5000);

    expect(useToastStore.getState().toasts).toHaveLength(0);
  });

  it('does not remove the toast before 5 seconds', () => {
    useToastStore.getState().addToast({ type: 'success', message: 'Still here' });

    vi.advanceTimersByTime(4999);

    expect(useToastStore.getState().toasts).toHaveLength(1);
  });
});
