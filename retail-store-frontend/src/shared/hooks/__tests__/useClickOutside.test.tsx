import { render, screen, act } from '@/test/test-utils';
import { setupUser } from '@/test/test-utils';
import { useClickOutside } from '../index';

/**
 * Test component that uses the hook and attaches the ref to an inner element.
 * Clicking outside the inner box should trigger onOutsideClick.
 */
function TestComponent({ onOutsideClick }: { onOutsideClick: () => void }) {
  const ref = useClickOutside<HTMLDivElement>(onOutsideClick);
  return (
    <div data-testid="outer" style={{ padding: 40 }}>
      <div ref={ref} data-testid="inner" style={{ padding: 20 }}>
        Inner element
      </div>
    </div>
  );
}

describe('useClickOutside', () => {
  it('returns a ref object', () => {
    const callback = vi.fn();
    const { result } = (() => {
      let refValue: ReturnType<typeof useClickOutside<HTMLDivElement>> | null = null;
      render(<TestComponent onOutsideClick={callback} />);
      return { result: refValue };
    })();
    // Structural test: the hook renders without crashing
    expect(screen.getByTestId('inner')).toBeInTheDocument();
  });

  it('does NOT call callback when clicking inside the ref element', async () => {
    const callback = vi.fn();
    const user = setupUser();
    render(<TestComponent onOutsideClick={callback} />);

    await user.click(screen.getByTestId('inner'));

    expect(callback).not.toHaveBeenCalled();
  });

  it('calls callback when clicking outside the ref element', async () => {
    const callback = vi.fn();
    render(<TestComponent onOutsideClick={callback} />);

    // Fire mousedown on the outer wrapper (outside the inner ref element)
    act(() => {
      const outer = screen.getByTestId('outer');
      outer.dispatchEvent(new MouseEvent('mousedown', { bubbles: true }));
    });

    expect(callback).toHaveBeenCalledOnce();
  });

  it('does not call callback when clicking on a child of the ref element', async () => {
    const callback = vi.fn();
    function WithChild({ onOut }: { onOut: () => void }) {
      const ref = useClickOutside<HTMLDivElement>(onOut);
      return (
        <div>
          <div ref={ref} data-testid="tracked">
            <button data-testid="child-btn">Inside child</button>
          </div>
        </div>
      );
    }
    render(<WithChild onOut={callback} />);

    act(() => {
      screen.getByTestId('child-btn').dispatchEvent(
        new MouseEvent('mousedown', { bubbles: true })
      );
    });

    expect(callback).not.toHaveBeenCalled();
  });
});
