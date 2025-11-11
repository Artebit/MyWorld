import axios from "axios";

export const http = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7105/api",
});

export const isMock = import.meta.env.VITE_MOCK === "1";
