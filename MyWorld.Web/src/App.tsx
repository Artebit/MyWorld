import { useEffect, useState } from "react";
import {
    getDimensions,
    getQuestions,
    startSession,
    submitAnswer,
    completeSession,
    getResult,
    Dimension,
    Question,
} from "./api";

const DEMO_USER_ID = "11111111-1111-1111-1111-111111111111";

type AnswersMap = Record<string, number>; // questionId -> value

export default function App() {
    const [dimensions, setDimensions] = useState<Dimension[]>([]);
    const [questions, setQuestions] = useState<Question[]>([]);
    const [sessionId, setSessionId] = useState<string | null>(null);
    const [answers, setAnswers] = useState<AnswersMap>({});
    const [result, setResult] = useState<any[] | null>(null);
    const [busy, setBusy] = useState(false);

    useEffect(() => {
        (async () => {
            setDimensions(await getDimensions());
            setQuestions(await getQuestions());
        })();
    }, []);

    const onStart = async () => {
        setBusy(true);
        try {
            const s = await startSession(DEMO_USER_ID);
            setSessionId(s.sessionId);
            setResult(null);
            setAnswers({});
        } finally {
            setBusy(false);
        }
    };

    const onChangeAnswer = (qid: string, val: number) => {
        setAnswers((a) => ({ ...a, [qid]: val }));
    };

    const onSaveAll = async () => {
        if (!sessionId) return;
        setBusy(true);
        try {
            // отправим только те ответы, которые пользователь задал
            const entries = Object.entries(answers);
            for (const [qid, val] of entries) {
                await submitAnswer(sessionId, qid, val);
            }
            await completeSession(sessionId);
            const r = await getResult(sessionId);
            setResult(r);
        } finally {
            setBusy(false);
        }
    };

    return (
        <div style={{ maxWidth: 900, margin: "40px auto", fontFamily: "system-ui, sans-serif" }}>
            <h1>MyWorld — демо</h1>

            <p>
                API: <code>{import.meta.env.VITE_API_BASE_URL}</code>
            </p>

            <div style={{ marginBottom: 20 }}>
                <button onClick={onStart} disabled={busy} style={{ padding: "8px 14px" }}>
                    {sessionId ? "Начать заново" : "Начать тест"}
                </button>
                {sessionId && <span style={{ marginLeft: 10 }}>Сессия: {sessionId}</span>}
            </div>

            <h2>Вопросы</h2>
            {!sessionId && <p>Нажми «Начать тест», чтобы отвечать.</p>}

            {sessionId &&
                dimensions.map((d) => (
                    <div key={d.id} style={{ marginBottom: 24, padding: 12, border: "1px solid #eee", borderRadius: 8 }}>
                        <h3 style={{ margin: 0 }}>{d.name}</h3>
                        <small style={{ color: "#666" }}>{d.description ?? " "}</small>

                        {questions
                            .filter((q) => q.dimensionId === d.id)
                            .sort((a, b) => a.order - b.order)
                            .map((q) => (
                                <div key={q.id} style={{ marginTop: 10 }}>
                                    <div>{q.text}</div>
                                    <input
                                        type="range"
                                        min={1}
                                        max={10}
                                        step={1}
                                        value={answers[q.id] ?? 5}
                                        onChange={(e) => onChangeAnswer(q.id, Number(e.target.value))}
                                        style={{ width: "100%" }}
                                    />
                                    <div>Значение: {answers[q.id] ?? 5}</div>
                                </div>
                            ))}
                    </div>
                ))}

            {sessionId && (
                <button onClick={onSaveAll} disabled={busy} style={{ padding: "8px 14px" }}>
                    Сохранить ответы и показать результат
                </button>
            )}

            {result && (
                <>
                    <h2>Результат</h2>
                    <pre style={{ background: "#f6f8fa", padding: 12, borderRadius: 8 }}>
                        {JSON.stringify(result, null, 2)}
                    </pre>
                    <p>Позже сюда легко добавить диаграмму (например, Recharts RadarChart).</p>
                </>
            )}
        </div>
    );
}
