import { useCallback, useEffect, useMemo, useState } from "react";
import { wheelApi } from "@/Services/wheel";
import type { Dimension, Question } from "@/lib/types";

export type AnswerState = {
    value: number | null;
    note: string;
    saved: boolean;
    saving: boolean;
    error?: string;
};

type AnswersMap = Record<string, AnswerState>;

const createInitialState = (questions: Question[]): AnswersMap => {
    return questions.reduce<AnswersMap>((acc, question) => {
        if (!acc[question.id]) {
            acc[question.id] = {
                value: null,
                note: "",
                saved: false,
                saving: false,
            };
        }
        return acc;
    }, {});
};

const ensureAnswer = (answers: AnswersMap, questionId: string): AnswerState => {
    if (answers[questionId]) {
        return answers[questionId];
    }
    return { value: null, note: "", saved: false, saving: false };
};

export function useWheelSession(sessionId?: string) {
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [questions, setQuestions] = useState<Question[]>([]);
    const [dimensions, setDimensions] = useState<Dimension[]>([]);
    const [answers, setAnswers] = useState<AnswersMap>({});
    const [completeBusy, setCompleteBusy] = useState(false);
    const [completeError, setCompleteError] = useState<string | null>(null);

    useEffect(() => {
        let active = true;
        setLoading(true);
        setError(null);
        (async () => {
            try {
                const [rawQuestions, dims] = await Promise.all([
                    wheelApi.getQuestions(),
                    wheelApi.getDimensions(),
                ]);
                if (!active) return;
                const ordered = [...rawQuestions].sort((a, b) => a.order - b.order);
                setQuestions(ordered);
                setDimensions(dims);
                setAnswers(prev => ({ ...createInitialState(ordered), ...prev }));
            } catch (err) {
                if (!active) return;
                const message = err instanceof Error ? err.message : "Не удалось загрузить данные упражнения";
                setError(message);
            } finally {
                if (active) setLoading(false);
            }
        })();

        return () => {
            active = false;
        };
    }, []);

    const setAnswerValue = useCallback((questionId: string, value: number | null) => {
        setAnswers(prev => {
            const current = ensureAnswer(prev, questionId);
            return {
                ...prev,
                [questionId]: {
                    ...current,
                    value,
                    saved: false,
                    saving: false,
                    error: undefined,
                },
            };
        });
    }, []);

    const setAnswerNote = useCallback((questionId: string, note: string) => {
        setAnswers(prev => {
            const current = ensureAnswer(prev, questionId);
            return {
                ...prev,
                [questionId]: {
                    ...current,
                    note,
                    saved: false,
                    saving: false,
                    error: undefined,
                },
            };
        });
    }, []);

    const saveAnswer = useCallback(async (questionId: string) => {
        if (!sessionId) {
            throw new Error("Сессия не найдена");
        }
        const current = ensureAnswer(answers, questionId);
        const hasValue = typeof current.value === "number";
        const note = current.note.trim();
        const hasNote = note.length > 0;

        if (!hasValue && !hasNote) {
            const message = "Добавьте оценку или комментарий, чтобы сохранить ответ.";
            setAnswers(prev => ({
                ...prev,
                [questionId]: { ...ensureAnswer(prev, questionId), error: message },
            }));
            throw new Error(message);
        }

        setAnswers(prev => ({
            ...prev,
            [questionId]: {
                ...ensureAnswer(prev, questionId),
                saving: true,
                error: undefined,
            },
        }));

        try {
            await wheelApi.submitAnswer(
                sessionId,
                questionId,
                hasValue ? current.value ?? undefined : undefined,
                hasNote ? note : undefined,
            );
            setAnswers(prev => {
                const latest = ensureAnswer(prev, questionId);
                const changed = latest.value !== current.value || latest.note !== current.note;
                return {
                    ...prev,
                    [questionId]: {
                        ...latest,
                        saved: !changed,
                        saving: false,
                        error: undefined,
                    },
                };
            });
        } catch (err) {
            const message = err instanceof Error ? err.message : "Не удалось сохранить ответ";
            setAnswers(prev => ({
                ...prev,
                [questionId]: {
                    ...ensureAnswer(prev, questionId),
                    saving: false,
                    saved: false,
                    error: message,
                },
            }));
            throw err;
        }
    }, [answers, sessionId]);

    const completeSession = useCallback(async () => {
        if (!sessionId) {
            throw new Error("Сессия не найдена");
        }
        setCompleteBusy(true);
        setCompleteError(null);
        try {
            await wheelApi.complete(sessionId);
        } catch (err) {
            const message = err instanceof Error ? err.message : "Не удалось завершить упражнение";
            setCompleteError(message);
            throw err;
        } finally {
            setCompleteBusy(false);
        }
    }, [sessionId]);

    const dimensionsMap = useMemo(() => {
        return dimensions.reduce<Record<string, Dimension>>((acc, item) => {
            acc[item.id] = item;
            return acc;
        }, {});
    }, [dimensions]);

    return {
        loading,
        error,
        questions,
        dimensions,
        dimensionsMap,
        answers,
        setAnswerValue,
        setAnswerNote,
        saveAnswer,
        completeSession,
        completeBusy,
        completeError,
    };
}

export type UseWheelSessionReturn = ReturnType<typeof useWheelSession>;
