import { render, screen } from '@/test/test-utils';
import { setupUser } from '@/test/test-utils';
import { Modal } from '@shared/components/ui';

describe('Modal', () => {
  it('does not render when isOpen is false', () => {
    render(<Modal isOpen={false} onClose={() => {}} title="My Modal">Modal body</Modal>);

    expect(screen.queryByText('My Modal')).not.toBeInTheDocument();
    expect(screen.queryByText('Modal body')).not.toBeInTheDocument();
  });

  it('renders title and children when isOpen is true', () => {
    render(<Modal isOpen onClose={() => {}} title="Confirm Delete">Are you sure?</Modal>);

    expect(screen.getByText('Confirm Delete')).toBeInTheDocument();
    expect(screen.getByText('Are you sure?')).toBeInTheDocument();
  });

  it('calls onClose when the X button is clicked', async () => {
    const onClose = vi.fn();
    const user = setupUser();
    render(<Modal isOpen onClose={onClose} title="Modal">Content</Modal>);

    // The X close button has no visible label — find it by its icon button role
    const buttons = screen.getAllByRole('button');
    await user.click(buttons[0]);

    expect(onClose).toHaveBeenCalledOnce();
  });

  it('calls onClose when the backdrop is clicked', async () => {
    const onClose = vi.fn();
    const user = setupUser();
    render(<Modal isOpen onClose={onClose} title="Modal">Content</Modal>);

    // The backdrop div has an onClick handler but is not a button/link.
    // Simulate a click on the element just behind the modal content.
    // The backdrop is a sibling of the modal content, both inside the outer div.
    const backdrop = document.querySelector('.fixed.inset-0.bg-black\\/50') as HTMLElement;
    if (backdrop) await user.click(backdrop);

    expect(onClose).toHaveBeenCalled();
  });

  it.each(['sm', 'md', 'lg'] as const)(
    'renders without error for size="%s"',
    (size) => {
      render(<Modal isOpen onClose={() => {}} title="Modal" size={size}>Content</Modal>);
      expect(screen.getByText('Modal')).toBeInTheDocument();
    }
  );
});
