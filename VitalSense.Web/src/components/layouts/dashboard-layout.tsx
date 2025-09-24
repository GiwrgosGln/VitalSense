import { Toaster } from "sonner";
import { AppSidebar } from "../app-sidebar";
import { SidebarInset, SidebarProvider, SidebarTrigger } from "../ui/sidebar";
import { Separator } from "../ui/separator";
import { ThemeModeToggle } from "../theme-mode-toggle";

export function DashboardLayout({ children }: { children: React.ReactNode }) {
  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <header className="flex h-16 shrink-0 items-center gap-2 border-b px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator
            orientation="vertical"
            className="mr-2 data-[orientation=vertical]:h-8"
          />
          <ThemeModeToggle />
        </header>
        <main>{children}</main>
      </SidebarInset>
      <Toaster />
    </SidebarProvider>
  );
}
