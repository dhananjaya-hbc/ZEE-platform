import { describe, expect, it, vi } from 'vitest';
import AppLayout from './layout';

const mockRedirect = vi.fn();
vi.mock('next/navigation', () => ({
  redirect: (path: string) => {
    mockRedirect(path);
    throw new Error(`NEXT_REDIRECT:${path}`);
  },
}));

const mockGetServerSession = vi.fn();
vi.mock('@/lib/auth-server', () => ({
  getServerSession: () => mockGetServerSession(),
}));

vi.mock('@/components/AppHeader', () => ({
  AppHeader: () => <div data-testid="app-header" />,
}));

vi.mock('@/components/BottomNav', () => ({
  BottomNav: () => <div data-testid="bottom-nav" />,
}));

describe('(app)/layout.tsx', () => {
  it('redirects unauthenticated anonymous visitors to /', async () => {
    mockGetServerSession.mockResolvedValue(null);

    await expect(
      AppLayout({ children: <div>Secret Content</div> })
    ).rejects.toThrow('NEXT_REDIRECT:/');

    expect(mockRedirect).toHaveBeenCalledWith('/');
  });

  it('renders layout and children for authenticated visitors', async () => {
    mockGetServerSession.mockResolvedValue({
      userId: 'usr-1',
      universityId: 'uni-1',
    });

    const element = await AppLayout({ children: <div data-testid="child-content">Content</div> });
    expect(element).toBeDefined();
  });
});
