import { useForm } from "react-hook-form";
import { Link, useSearchParams } from "react-router-dom";
import { zodResolver } from "@hookform/resolvers/zod";

import { useLogin, loginInputSchema, type LoginInput } from "@/lib/auth";
import { paths } from "@/config/paths";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

type LoginFormProps = {
  onSuccess: () => void;
};

export const LoginForm = ({ onSuccess }: LoginFormProps) => {
  const login = useLogin({
    onSuccess,
  });
  const [searchParams] = useSearchParams();
  const redirectTo = searchParams.get("redirectTo");

  const form = useForm<LoginInput>({
    resolver: zodResolver(loginInputSchema),
    defaultValues: {
      username: "",
      password: "",
    },
  });

  const onSubmit = (values: LoginInput) => {
    login.mutate(values);
  };

  return (
    <div>
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <FormField
            control={form.control}
            name="username"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Username</FormLabel>
                <FormControl>
                  <Input placeholder="username" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="password"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Password</FormLabel>
                <FormControl>
                  <Input type="password" placeholder="password" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit" className="w-full" disabled={login.isPending}>
            {login.isPending ? "Logging in..." : "Log in"}
          </Button>
        </form>
      </Form>

      <div className="mt-2 flex items-center justify-end">
        <div className="text-sm">
          <Link
            to={
              paths.auth.register.getHref?.(redirectTo) ||
              `/auth/register${redirectTo ? `?redirectTo=${redirectTo}` : ""}`
            }
            className="font-medium text-blue-600 hover:text-blue-500"
          >
            Register
          </Link>
        </div>
      </div>
    </div>
  );
};
