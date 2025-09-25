import Axios from "axios";
import { useAuthStore } from "@/store/auth-store";
import { toast } from "sonner";

const isDevelopment = import.meta.env.DEV;

export const api = Axios.create({
  baseURL: isDevelopment
    ? "http://localhost:5223/v1"
    : "https://api.vitalsense.gr/v1",
  headers: {
    Accept: "application/json",
    "Content-Type": "application/json",
  },
  withCredentials: true,
});

api.interceptors.request.use(
  (request) => {
    const accessToken = useAuthStore.getState().accessToken;
    if (accessToken) request.headers["Authorization"] = `Bearer ${accessToken}`;
    return request;
  },
  (error) => {
    return Promise.reject(error);
  }
);

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (
      error.response &&
      error.response.status === 401 &&
      !originalRequest._retry &&
      !error.config.url.endsWith("/auth/refresh")
    ) {
      try {
        useAuthStore.getState().setIsRefreshing(true);
        const response = await api.post("/auth/refresh", {
          withCredentials: true,
        });
        const newAccessToken = response.data.accessToken;
        if (newAccessToken) {
          useAuthStore.getState().setAccessToken(newAccessToken);
          originalRequest.headers["Authorization"] = `Bearer ${newAccessToken}`;
          return api(originalRequest);
        }
      } catch (refreshError) {
        toast("Your session has expired");
      } finally {
        useAuthStore.getState().setIsRefreshing(false);
      }
    }
    return Promise.reject(error);
  }
);
