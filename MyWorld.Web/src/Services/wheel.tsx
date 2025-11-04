import { http } from "./http";
import type { Dimension, Question } from "@/lib/types";

export const wheelApi = {
    getDimensions: () => http.get<Dimension[]>("/dimensions").then(r => r.data),
    getQuestions: () => http.get<Question[]>("/questions").then(r => r.data),
    startSession: (userId: string) => http.post("/sessions/start", null, { params: { userId } }).then(r => r.data),
    submitAnswer: (sessionId: string, questionId: string, value?: number, text?: string) =>
        http.post(`/sessions/${sessionId}/answers`, { questionId, value, text }).then(r => r.data),
    complete: (sessionId: string) => http.post(`/sessions/${sessionId}/complete`).then(r => r.data),
    getResult: (sessionId: string) => http.get(`/sessions/${sessionId}/result`).then(r => r.data)
};
