import { render, screen } from '@/test/test-utils';
import { setupUser } from '@/test/test-utils';
import {
  Badge,
  Alert,
  Toggle,
  ActionMenu,
  SearchInput,
  FilterPillBar,
  Card,
  EmptyState,
  Avatar,
  Spinner,
} from '@shared/components/ui';
import { Package } from 'lucide-react';

// ── Badge ─────────────────────────────────────────────────────────────────────

describe('Badge', () => {
  it('renders its children', () => {
    render(<Badge>Active</Badge>);
    expect(screen.getByText('Active')).toBeInTheDocument();
  });

  it.each(['default', 'success', 'warning', 'danger', 'info'] as const)(
    'renders variant "%s" without crashing',
    (variant) => {
      render(<Badge variant={variant}>Label</Badge>);
      expect(screen.getByText('Label')).toBeInTheDocument();
    }
  );
});

// ── Alert ─────────────────────────────────────────────────────────────────────

describe('Alert', () => {
  it('renders the message text', () => {
    render(<Alert message="Something went wrong" />);
    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
  });

  it.each(['error', 'warning', 'success', 'info'] as const)(
    'renders variant "%s" without crashing',
    (variant) => {
      render(<Alert message="Alert text" variant={variant} />);
      expect(screen.getByText('Alert text')).toBeInTheDocument();
    }
  );
});

// ── Toggle ────────────────────────────────────────────────────────────────────

describe('Toggle', () => {
  it('renders with aria-checked reflecting the checked prop', () => {
    render(<Toggle checked={false} onChange={() => {}} />);
    expect(screen.getByRole('switch')).toHaveAttribute('aria-checked', 'false');
  });

  it('reflects checked=true in aria-checked', () => {
    render(<Toggle checked onChange={() => {}} />);
    expect(screen.getByRole('switch')).toHaveAttribute('aria-checked', 'true');
  });

  it('calls onChange with the inverted value when clicked', async () => {
    const onChange = vi.fn();
    const user = setupUser();
    render(<Toggle checked={false} onChange={onChange} />);

    await user.click(screen.getByRole('switch'));

    expect(onChange).toHaveBeenCalledWith(true);
  });

  it('does not call onChange when disabled', async () => {
    const onChange = vi.fn();
    const user = setupUser();
    render(<Toggle checked={false} onChange={onChange} disabled />);

    await user.click(screen.getByRole('switch'));

    expect(onChange).not.toHaveBeenCalled();
  });
});

// ── ActionMenu ────────────────────────────────────────────────────────────────

describe('ActionMenu', () => {
  const items = [
    { label: 'Edit', onClick: vi.fn() },
    { label: 'Delete', onClick: vi.fn(), variant: 'danger' as const },
  ];

  it('does not show items before the trigger is clicked', () => {
    render(<ActionMenu items={items} />);

    expect(screen.queryByText('Edit')).not.toBeInTheDocument();
  });

  it('shows items after the trigger button is clicked', async () => {
    const user = setupUser();
    render(<ActionMenu items={items} />);

    await user.click(screen.getByRole('button'));

    expect(screen.getByText('Edit')).toBeInTheDocument();
    expect(screen.getByText('Delete')).toBeInTheDocument();
  });

  it('calls the item onClick and closes the menu', async () => {
    const onEdit = vi.fn();
    const user = setupUser();
    render(<ActionMenu items={[{ label: 'Edit', onClick: onEdit }]} />);

    await user.click(screen.getByRole('button'));          // open
    await user.click(screen.getByRole('button', { name: 'Edit' })); // select

    expect(onEdit).toHaveBeenCalledOnce();
    expect(screen.queryByText('Edit')).not.toBeInTheDocument(); // closed
  });
});

// ── SearchInput ───────────────────────────────────────────────────────────────

describe('SearchInput', () => {
  it('renders with the current value', () => {
    render(<SearchInput value="widget" onChange={() => {}} />);
    expect(screen.getByDisplayValue('widget')).toBeInTheDocument();
  });

  it('calls onChange when the user types', async () => {
    const onChange = vi.fn();
    const user = setupUser();
    render(<SearchInput value="" onChange={onChange} placeholder="Search…" />);

    await user.type(screen.getByPlaceholderText('Search…'), 'abc');

    expect(onChange).toHaveBeenCalled();
  });
});

// ── FilterPillBar ─────────────────────────────────────────────────────────────

describe('FilterPillBar', () => {
  const options = [
    { key: 'all', label: 'All' },
    { key: 'active', label: 'Active' },
    { key: 'inactive', label: 'Inactive' },
  ];

  it('renders all pill options', () => {
    render(<FilterPillBar options={options} value="all" onChange={() => {}} />);

    expect(screen.getByText('All')).toBeInTheDocument();
    expect(screen.getByText('Active')).toBeInTheDocument();
    expect(screen.getByText('Inactive')).toBeInTheDocument();
  });

  it('calls onChange with the selected key when a pill is clicked', async () => {
    const onChange = vi.fn();
    const user = setupUser();
    render(<FilterPillBar options={options} value="all" onChange={onChange} />);

    await user.click(screen.getByText('Active'));

    expect(onChange).toHaveBeenCalledWith('active');
  });
});

// ── Card ──────────────────────────────────────────────────────────────────────

describe('Card', () => {
  it('renders children', () => {
    render(<Card>Card content</Card>);
    expect(screen.getByText('Card content')).toBeInTheDocument();
  });

  it('renders the title when provided', () => {
    render(<Card title="My Card">Content</Card>);
    expect(screen.getByText('My Card')).toBeInTheDocument();
  });

  it('renders the subtitle when provided', () => {
    render(<Card title="Title" subtitle="A description">Content</Card>);
    expect(screen.getByText('A description')).toBeInTheDocument();
  });

  it('renders without a header when title and actions are absent', () => {
    const { container } = render(<Card>Just content</Card>);
    // No header border div should be present
    expect(container.querySelector('.border-b')).not.toBeInTheDocument();
  });
});

// ── EmptyState ────────────────────────────────────────────────────────────────

describe('EmptyState', () => {
  it('renders the title', () => {
    render(<EmptyState title="No products found" />);
    expect(screen.getByText('No products found')).toBeInTheDocument();
  });

  it('renders description when provided', () => {
    render(<EmptyState title="Empty" description="Add your first item to get started." />);
    expect(screen.getByText('Add your first item to get started.')).toBeInTheDocument();
  });

  it('renders the action when provided', () => {
    render(
      <EmptyState
        title="Empty"
        action={<button>Add item</button>}
      />
    );
    expect(screen.getByRole('button', { name: 'Add item' })).toBeInTheDocument();
  });
});

// ── Avatar ────────────────────────────────────────────────────────────────────

describe('Avatar', () => {
  it('renders initials in uppercase', () => {
    render(<Avatar initials="jd" />);
    expect(screen.getByText('JD')).toBeInTheDocument();
  });
});

// ── Spinner ───────────────────────────────────────────────────────────────────

describe('Spinner', () => {
  it('renders without crashing', () => {
    const { container } = render(<Spinner />);
    expect(container.firstChild).toBeInTheDocument();
  });
});
