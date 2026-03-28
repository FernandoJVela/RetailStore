import { Outlet } from 'react-router-dom';
import { Sidebar } from '@shared/components/navigation/Sidebar';
import { Topbar } from '@shared/components/navigation/Topbar';
import { useSidebarStore } from '@shared/store/sidebar-store';
import { cn } from '@shared/lib/utils';
 
export function MainLayout() {
  const collapsed = useSidebarStore((s) => s.collapsed);
 
  return (
    <div className="flex min-h-screen">
      <Sidebar />
 
      {/*
        Responsive main content area:
        - Mobile (< lg):  ml-0, full width (sidebar is an overlay)
        - Desktop expanded: ml-[260px]
        - Desktop collapsed: ml-[68px]
        Transition animates the margin change smoothly.
      */}
      <main
        className={cn(
          'flex-1 min-w-0 px-4 py-4 sm:px-6 sm:py-6 lg:px-8 lg:py-6 transition-[margin] duration-300',
          'ml-0',
          collapsed ? 'lg:ml-[68px]' : 'lg:ml-[260px]'
        )}
      >
        <Topbar />
        <Outlet />
      </main>
    </div>
  );
}