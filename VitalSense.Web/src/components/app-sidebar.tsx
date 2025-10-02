import {
  Calendar,
  Frame,
  LayoutDashboard,
  PieChart,
  Users,
  FileQuestionMark,
} from "lucide-react";

import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from "@/components/ui/sidebar";
import { paths } from "@/config/paths";
import { Link } from "react-router-dom";

import logo from "@/assets/logo.png";
import { NavUser } from "./nav-user";
import { useAuthStore } from "@/store/auth-store";

const items = [
  {
    title: "Dashboard",
    url: paths.app.dashboard.path,
    icon: LayoutDashboard,
  },
  {
    title: "Calendar",
    url: paths.app.calendar.path,
    icon: Calendar,
  },
  {
    title: "Clients",
    url: paths.app.clients.path,
    icon: Users,
  },
  {
    title: "Questionnaires",
    url: paths.app.questionnaires.path,
    icon: FileQuestionMark,
  },
];

const quickActions = [
  {
    name: "Create Client",
    url: "#",
    icon: Frame,
  },
  {
    name: "Start Questionnaire",
    url: "#",
    icon: PieChart,
  },
  {
    name: "Create Meal Plan",
    url: "#",
    icon: Map,
  },
];

export function AppSidebar() {
  const user = useAuthStore((state) => state.user);

  const navUser = user
    ? {
        name: user.username,
        email: user.email,
        avatar: "",
      }
    : {
        name: "",
        email: "",
        avatar: "",
      };
  return (
    <Sidebar>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton size="lg" asChild>
              <a href="#">
                <div className="flex aspect-square size-8 items-center justify-center rounded-lg">
                  <img src={logo} alt="Vital Sense" />
                </div>
                <div className="grid flex-1 text-left text-sm leading-tight">
                  <span className="truncate font-medium">Vital Sense</span>
                  <span className="truncate text-xs">Management System</span>
                </div>
              </a>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Vital Sense</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {items.map((item) => (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton asChild>
                    <Link to={item.url}>
                      <item.icon />
                      <span>{item.title}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
        <SidebarGroup>
          <SidebarGroupLabel>Quick Actions</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {quickActions.map((item) => (
                <SidebarMenuItem key={item.name}>
                  <SidebarMenuButton asChild>
                    <Link to={item.url}>
                      <span>{item.name}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
      <SidebarFooter>
        <NavUser user={navUser} />
      </SidebarFooter>
    </Sidebar>
  );
}
