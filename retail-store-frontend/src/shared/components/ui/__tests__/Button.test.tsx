import { render, screen } from '@/test/test-utils';
import { setupUser } from '@/test/test-utils';
import { Button } from '@shared/components/ui';

describe('Button', () => {
  it('renders its children as the accessible name', () => {
    render(<Button>Save</Button>);

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
  });

  it('is enabled by default', () => {
    render(<Button>Click</Button>);

    expect(screen.getByRole('button')).not.toBeDisabled();
  });

  it('is disabled when the disabled prop is set', () => {
    render(<Button disabled>Submit</Button>);

    expect(screen.getByRole('button')).toBeDisabled();
  });

  it('is disabled and shows a spinner when loading is true', () => {
    render(<Button loading>Loading</Button>);

    const btn = screen.getByRole('button');
    expect(btn).toBeDisabled();
    // The loading SVG spinner is rendered inside the button
    expect(btn.querySelector('svg')).toBeInTheDocument();
  });

  it('calls onClick when clicked', async () => {
    const onClick = vi.fn();
    const user = setupUser();
    render(<Button onClick={onClick}>Click me</Button>);

    await user.click(screen.getByRole('button'));

    expect(onClick).toHaveBeenCalledOnce();
  });

  it('does not call onClick when disabled', async () => {
    const onClick = vi.fn();
    const user = setupUser();
    render(<Button disabled onClick={onClick}>Disabled</Button>);

    await user.click(screen.getByRole('button'));

    expect(onClick).not.toHaveBeenCalled();
  });

  it('does not call onClick when loading', async () => {
    const onClick = vi.fn();
    const user = setupUser();
    render(<Button loading onClick={onClick}>Loading</Button>);

    await user.click(screen.getByRole('button'));

    expect(onClick).not.toHaveBeenCalled();
  });

  it.each(['primary', 'secondary', 'danger', 'ghost', 'outline'] as const)(
    'renders the %s variant without crashing',
    (variant) => {
      render(<Button variant={variant}>Button</Button>);
      expect(screen.getByRole('button')).toBeInTheDocument();
    }
  );
});
