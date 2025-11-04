import { http } from "./http";
import type { Dimension, Question, ResultItem } from "@/lib/types";

export async function getDimensions() {
    const { data } = await http.get<Dimension[]>("/dimensions");
    return data;
}

export async function getQuestions() {
    const { data } = await http.get<Question[]>("/questions");
    return data;
}

export async function startSession(userId: string) {
    const { data } = await http.post<{ sessionId: string }>(
        "/sessions/start",
        null,
        { params: { userId } }
    );
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
    const { data } = await http.get<ResultItem[]>(`/sessions/${sessionId}/result`);
    return data;
}
