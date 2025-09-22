import type { AuthResponse, User } from "@/types/api";
import { configureAuth } from "react-query-auth";
import { api } from "./api-client";
import z from "zod";
import { Navigate, useLocation } from "react-router-dom";
import { paths } from "@/config/paths";
import { useAuthStore } from "@/store/auth-store";

const getUser = async (): Promise<User> => {
  const response = await api.get("auth/me");

  return response.data;
};

const logout = (): Promise<void> => {
  return api.post("/auth/logout");
};

export const loginInputSchema = z.object({
  username: z.string().min(1, "Required"),
  password: z.string().min(5, "Required"),
});

export type LoginInput = z.infer<typeof loginInputSchema>;

const loginWithUsernameAndPassword = (
  data: LoginInput
): Promise<AuthResponse> => {
  return api.post("/auth/login", data).then((res) => res.data);
};

export const registerInputSchema = z
  .object({
    username: z
      .string()
      .min(6, "Username must be at least 6 characters long.")
      .max(50, "Username cannot exceed 50 characters.")
      .regex(
        /^[a-zA-Z0-9._-]+$/,
        "Username can only contain letters, numbers, dots, underscores, or hyphens."
      ),
    email: z
      .string()
      .min(1, "Email is required.")
      .email("Invalid email format.")
      .max(255, "Email cannot exceed 255 characters."),
    password: z
      .string()
      .min(8, "Password must be at least 8 characters long.")
      .max(100, "Password cannot exceed 100 characters.")
      .regex(/[A-Z]/, "Password must contain at least one uppercase letter.")
      .regex(/[a-z]/, "Password must contain at least one lowercase letter.")
      .regex(/[0-9]/, "Password must contain at least one digit.")
      .regex(
        /[^a-zA-Z0-9]/,
        "Password must contain at least one special character."
      ),
    confirmPassword: z.string().min(1, "Confirm password is required."),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "The password and confirmation password do not match.",
    path: ["confirmPassword"],
  });

export type RegisterInput = z.infer<typeof registerInputSchema>;

const registerWithUsernameEmailAndPassword = (
  data: RegisterInput
): Promise<AuthResponse> => {
  return api.post("/auth/register", data).then((res) => res.data);
};

const authConfig = {
  userFn: getUser,
  loginFn: async (data: LoginInput) => {
    const response = await loginWithUsernameAndPassword(data);
    useAuthStore.getState().setAccessToken(response.accessToken);
    return response.user;
  },
  registerFn: async (data: RegisterInput) => {
    const response = await registerWithUsernameEmailAndPassword(data);
    return response.user;
  },
  logoutFn: logout,
};

export const { useUser, useLogin, useLogout, useRegister, AuthLoader } =
  configureAuth(authConfig);

export const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const user = useUser();
  const location = useLocation();
  const isRefreshing = useAuthStore((s) => s.isRefreshing);

  if (isRefreshing || user.isLoading) {
    return <h1>Refreshing...</h1>;
  }

  if (!user.data) {
    return (
      <Navigate to={paths.auth.login.getHref(location.pathname)} replace />
    );
  }
  return children;
};
