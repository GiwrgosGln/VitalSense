import { Toaster } from "sonner";
import { AppSidebar } from "../app-sidebar";
import { SidebarProvider } from "../ui/sidebar";
import { SiteHeader } from "../site-header";

export function DashboardLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="[--header-height:calc(--spacing(14))]">
      <SidebarProvider className="flex flex-col">
        <SiteHeader />
        <div className="flex flex-1">
          <AppSidebar />
          <main>{children}</main>
        </div>
        <Toaster />
      </SidebarProvider>
    </div>
  );
}
