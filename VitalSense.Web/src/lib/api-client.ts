import Axios from "axios";

export const api = Axios.create({
  baseURL: "https://api.vitalsense.gr/v1",
  headers: {
    Accept: "application/json",
    "Content-Type": "application/json",
  },
  withCredentials: false,
});
