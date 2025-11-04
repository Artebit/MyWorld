import axios from "axios";

export const http = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5135/api",
});

export const isMock = import.meta.env.VITE_MOCK === "1";
