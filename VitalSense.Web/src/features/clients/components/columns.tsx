import type { Client } from "@/types/api";

import type { ColumnDef } from "@tanstack/react-table";
import { MoreHorizontal } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { formatDate } from "@/utils/format";
import { useDeleteClient } from "../api/delete-client";

export const columns: ColumnDef<Client>[] = [
  {
    accessorKey: "firstName",
    header: "First Name",
  },
  {
    accessorKey: "lastName",
    header: "Last Name",
  },
  {
    accessorKey: "email",
    header: "Email",
  },
  {
    accessorKey: "phone",
    header: "Phone",
  },
  {
    accessorKey: "dateOfBirth",
    header: "Date Of Birth",
    cell: ({ getValue }) => formatDate(getValue() as string | undefined),
  },
  {
    accessorKey: "gender",
    header: "Gender",
  },
  {
    accessorKey: "notes",
    header: "Notes",
    cell: ({ getValue }) => (
      <div
        style={{
          maxWidth: "200px",
          whiteSpace: "nowrap",
          overflow: "hidden",
          textOverflow: "ellipsis",
        }}
        title={getValue() as string}
      >
        {getValue() as string}
      </div>
    ),
  },
  {
    accessorKey: "createdAt",
    header: "Created At",
    cell: ({ getValue }) => formatDate(getValue() as string | undefined),
  },
  {
    id: "actions",
    cell: ({ row }) => {
      const client = row.original;
      const { mutate: deleteClient, isPending } = useDeleteClient();

      const handleDelete = () => {
        if (window.confirm("Are you sure you want to delete this client?")) {
          deleteClient({ clientId: client.id });
        }
      };

      return (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" className="h-8 w-8 p-0">
              <span className="sr-only">Open menu</span>
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuLabel>Actions</DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              onClick={() => navigator.clipboard.writeText(client.id)}
            >
              View Client
            </DropdownMenuItem>
            <DropdownMenuItem>Edit Client</DropdownMenuItem>
            <DropdownMenuItem>Create Meal Plan</DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              className="text-red-500"
              onClick={handleDelete}
              disabled={isPending}
            >
              {isPending ? "Deleting..." : "Delete Client"}
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      );
    },
  },
];
