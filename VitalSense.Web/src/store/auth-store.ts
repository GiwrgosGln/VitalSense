import { create } from "zustand";

interface AuthState {
  accessToken: string | null;
  isRefreshing: boolean;
  setAccessToken: (token: string | null) => void;
  setIsRefreshing: (refreshing: boolean) => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  accessToken: null,
  isRefreshing: false,
  setAccessToken: (token) => set({ accessToken: token }),
  setIsRefreshing: (refreshing) => set({ isRefreshing: refreshing }),
}));
