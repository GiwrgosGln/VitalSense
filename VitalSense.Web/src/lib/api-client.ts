import Axios from "axios";
import { useAuthStore } from "@/store/auth-store";

export const api = Axios.create({
  baseURL: "https://api.vitalsense.gr/v1",
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
