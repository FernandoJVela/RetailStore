import { render, screen } from '@/test/test-utils';
import { setupUser } from '@/test/test-utils';
import { Input } from '@shared/components/ui';

describe('Input', () => {
  it('renders a text input', () => {
    render(<Input />);

    expect(screen.getByRole('textbox')).toBeInTheDocument();
  });

  it('renders a label when the label prop is set', () => {
    render(<Input label="Email address" />);

    expect(screen.getByLabelText('Email address')).toBeInTheDocument();
  });

  it('associates the label with the input via htmlFor/id', () => {
    render(<Input label="Username" />);

    // getByLabelText only succeeds when the label is properly associated
    const input = screen.getByLabelText('Username');
    expect(input.tagName).toBe('INPUT');
  });

  it('shows an error message when the error prop is set', () => {
    render(<Input error="This field is required" />);

    expect(screen.getByText('This field is required')).toBeInTheDocument();
  });

  it('does not show an error paragraph when error prop is absent', () => {
    render(<Input />);

    expect(screen.queryByRole('paragraph')).not.toBeInTheDocument();
  });

  it('calls onChange when the user types', async () => {
    const onChange = vi.fn();
    const user = setupUser();
    render(<Input onChange={onChange} />);

    await user.type(screen.getByRole('textbox'), 'hello');

    expect(onChange).toHaveBeenCalled();
  });

  it('forwards the value prop (controlled)', () => {
    render(<Input value="preset value" onChange={() => {}} />);

    expect(screen.getByDisplayValue('preset value')).toBeInTheDocument();
  });

  it('respects the disabled prop', () => {
    render(<Input disabled />);

    expect(screen.getByRole('textbox')).toBeDisabled();
  });
});
