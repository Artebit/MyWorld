// src/api.ts
import axios from "axios";

const BASE = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5135/api";
console.log("[API]", BASE);

const http = axios.create({ baseURL: BASE });

// dimensions
export async function getDimensions() {
    const { data } = await http.get("/dimensions");
    return data;
}

// questions
export async function getQuestions() {
    const { data } = await http.get("/questions");
    return data;
}

// sessions
export async function startSession(userId: string) {
    const { data } = await http.post("/sessions/start", null, { params: { userId } });
    return data;
}
export async function submitAnswer(sessionId: string, questionId: string, value?: number, text?: string) {
    const { data } = await http.post(`/sessions/${sessionId}/answers`, { questionId, value, text });
    return data;
}
export async function completeSession(sessionId: string) {
    const { data } = await http.post(`/sessions/${sessionId}/complete`);
    return data;
}
export async function getResult(sessionId: string) {
    const { data } = await http.get(`/sessions/${sessionId}/result`);
    return data;
}
