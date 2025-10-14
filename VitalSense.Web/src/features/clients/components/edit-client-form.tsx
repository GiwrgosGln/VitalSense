import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { ChevronDownIcon } from "lucide-react";
import {
  updateClientInputSchema,
  type UpdateClientInput,
  useUpdateClient,
} from "../api/update-client";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Input } from "@/components/ui/input";
import {
  Sheet,
  SheetClose,
  SheetContent,
  SheetFooter,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Textarea } from "@/components/ui/textarea";
import type { Client } from "@/types/api";

interface EditClientFormProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  client: Client;
}

export default function EditClientForm({
  open,
  onOpenChange,
  client,
}: EditClientFormProps) {
  const [datePopoverOpen, setDatePopoverOpen] = useState(false);

  const updateClientMutation = useUpdateClient({
    mutationConfig: {
      onSuccess: () => {
        toast.success("Client updated successfully.");
        onOpenChange(false);
      },
      onError: (error) => {
        toast.error("Failed to update client.", {
          description: error.message,
        });
      },
    },
  });

  const form = useForm<UpdateClientInput>({
    resolver: zodResolver(updateClientInputSchema),
    defaultValues: {
      firstName: client.firstName || "",
      lastName: client.lastName || "",
      email: client.email || "",
      phone: client.phone || "",
      dateOfBirth: client.dateOfBirth || "",
      gender: client.gender || "",
      notes: client.notes || "",
    },
  });

  const onSubmit = (data: UpdateClientInput) => {
    updateClientMutation.mutate({ data, clientId: client.id });
  };

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent>
        <SheetHeader>
          <SheetTitle>Edit Client</SheetTitle>
        </SheetHeader>
        <Form {...form}>
          <form
            id="edit-client-form"
            onSubmit={form.handleSubmit(onSubmit)}
            className="grid flex-1 auto-rows-min gap-4 px-4"
          >
            <FormField
              control={form.control}
              name="firstName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>First Name</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="Enter the first name..."
                      {...field}
                      disabled={updateClientMutation.isPending}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="lastName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Last Name</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="Enter the last name..."
                      {...field}
                      disabled={updateClientMutation.isPending}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Email</FormLabel>
                  <FormControl>
                    <Input
                      type="email"
                      placeholder="Enter the email..."
                      {...field}
                      disabled={updateClientMutation.isPending}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="phone"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Phone</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="Enter the phone..."
                      {...field}
                      disabled={updateClientMutation.isPending}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="dateOfBirth"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Date of birth</FormLabel>
                  <Popover
                    open={datePopoverOpen}
                    onOpenChange={setDatePopoverOpen}
                  >
                    <PopoverTrigger asChild>
                      <Button
                        variant="outline"
                        className="w-full justify-between font-normal"
                        type="button"
                      >
                        {field.value
                          ? new Date(field.value).toLocaleDateString()
                          : "Select date"}
                        <ChevronDownIcon />
                      </Button>
                    </PopoverTrigger>
                    <PopoverContent
                      className="w-full overflow-hidden p-0"
                      align="start"
                    >
                      <Calendar
                        mode="single"
                        selected={
                          field.value ? new Date(field.value) : undefined
                        }
                        onSelect={(date) => {
                          field.onChange(date ? date.toISOString() : "");
                          setDatePopoverOpen(false);
                        }}
                      />
                    </PopoverContent>
                  </Popover>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="gender"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Gender</FormLabel>
                  <Select
                    value={field.value}
                    onValueChange={field.onChange}
                    disabled={updateClientMutation.isPending}
                  >
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Select Gender" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectItem value="male">Male</SelectItem>
                      <SelectItem value="female">Female</SelectItem>
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="notes"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Notes</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Enter notes..."
                      {...field}
                      disabled={updateClientMutation.isPending}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </form>
        </Form>
        <SheetFooter>
          <Button
            type="submit"
            form="edit-client-form"
            disabled={updateClientMutation.isPending}
          >
            {updateClientMutation.isPending ? "Updating..." : "Update Client"}
          </Button>
          <SheetClose asChild>
            <Button variant="outline" type="button">
              Close
            </Button>
          </SheetClose>
        </SheetFooter>
      </SheetContent>
    </Sheet>
  );
}
