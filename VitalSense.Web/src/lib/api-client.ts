import Axios from "axios";
import { useAuthStore } from "@/store/auth-store";

const isDevelopment = import.meta.env.DEV;
const baseURL = isDevelopment
  ? "http://localhost:5223/v1"
  : "https://api.vitalsense.gr/v1";

export const api = Axios.create({
  baseURL,
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
