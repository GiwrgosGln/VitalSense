import { DashboardLayout } from "@/components/layouts/dashboard-layout";
import { Outlet } from "react-router-dom";

export const ErrorBoundary = () => {
  return <div>Something went wrong!</div>;
};

const AppRoot = () => {
  return (
    <DashboardLayout>
      <Outlet />
    </DashboardLayout>
  );
};

export default AppRoot;
