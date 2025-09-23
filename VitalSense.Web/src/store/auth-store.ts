import type { User } from "@/types/api";
import { create } from "zustand";

interface AuthState {
  accessToken: string | null;
  isRefreshing: boolean;
  user: User | null;
  setAccessToken: (token: string | null) => void;
  setIsRefreshing: (refreshing: boolean) => void;
  setUser: (user: User | null) => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  accessToken: null,
  isRefreshing: false,
  user: null,
  setAccessToken: (token) => set({ accessToken: token }),
  setIsRefreshing: (refreshing) => set({ isRefreshing: refreshing }),
  setUser: (user) => set({ user }),
}));
