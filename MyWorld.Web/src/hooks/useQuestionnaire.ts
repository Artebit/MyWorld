import { useEffect, useState } from "react";
import { getDimensions, getQuestions, startSession, submitAnswer, completeSession, getResult } from "@/services/api";
import type { Dimension, Question, ResultItem } from "@/lib/types";
type AnswersMap = Record<string, number>;

export function useQuestionnaire() {
    const [dimensions, setDimensions] = useState<Dimension[]>([]);
    const [questions, setQuestions] = useState<Question[]>([]);
    const [sessionId, setSessionId] = useState<string | null>(null);
    const [answers, setAnswers] = useState<AnswersMap>({});
    const [result, setResult] = useState<ResultItem[] | null>(null);
    const [busy, setBusy] = useState(false);

    useEffect(() => {
        (async () => {
            setDimensions(await getDimensions());
            setQuestions(await getQuestions());
        })();
    }, []);

    const start = async () => {
        setBusy(true);
        try {
            const uid = import.meta.env.VITE_DEMO_USER_ID ?? "11111111-1111-1111-1111-111111111111";
            const s = await startSession(uid);
            setSessionId(s.sessionId);
            setAnswers({});
            setResult(null);
        } finally {
            setBusy(false);
        }
    };

    const changeAnswer = (qid: string, val: number) => setAnswers(a => ({ ...a, [qid]: val }));

    const saveAll = async () => {
        if (!sessionId) return;
        setBusy(true);
        try {
            for (const [qid, val] of Object.entries(answers)) {
                await submitAnswer(sessionId, qid, val);
            }
            await completeSession(sessionId);
            setResult(await getResult(sessionId));
        } finally {
            setBusy(false);
        }
    };

    return { dimensions, questions, sessionId, answers, result, busy, start, changeAnswer, saveAll };
}
