import { useClients } from "@/features/clients/api/get-clients";
import { ClientsTable } from "@/features/clients/components/client-table";
import { columns } from "@/features/clients/components/columns";

const ClientsRoute = () => {
  const clientsQuery = useClients();

  const data = clientsQuery.data?.data;

  if (!data) return null;

  return <ClientsTable data={data} columns={columns} />;
};

export default ClientsRoute;
