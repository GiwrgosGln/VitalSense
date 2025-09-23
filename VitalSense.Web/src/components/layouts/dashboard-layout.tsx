import { Toaster } from "sonner";
import { AppSidebar } from "../app-sidebar";
import { SidebarProvider } from "../ui/sidebar";

export function DashboardLayout({ children }: { children: React.ReactNode }) {
  return (
    <SidebarProvider>
      <AppSidebar />
      <main>{children}</main>
      <Toaster />
    </SidebarProvider>
  );
}
