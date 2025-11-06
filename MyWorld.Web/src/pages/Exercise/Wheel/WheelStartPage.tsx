import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import useAuth from "@hooks/useAuth";
import { wheelApi } from "@/Services/wheel";
import type { Dimension, Question } from "@/lib/types";

export default function WheelStartPage() {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [questions, setQuestions] = useState<Question[]>([]);
    const [dimensions, setDimensions] = useState<Dimension[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [starting, setStarting] = useState(false);
    const [startError, setStartError] = useState<string | null>(null);

    useEffect(() => {
        let active = true;
        setLoading(true);
        (async () => {
            try {
                const [rawQuestions, dims] = await Promise.all([
                    wheelApi.getQuestions(),
                    wheelApi.getDimensions(),
                ]);
                if (!active) return;
                setQuestions(rawQuestions.sort((a, b) => a.order - b.order));
                setDimensions(dims);
            } catch (err) {
                if (!active) return;
                const message = err instanceof Error ? err.message : "Не удалось загрузить упражнение";
                setError(message);
            } finally {
                if (active) setLoading(false);
            }
        })();

        return () => {
            active = false;
        };
    }, []);

    const firstQuestionId = useMemo(() => questions[0]?.id, [questions]);

    const onStart = async () => {
        if (!user) return;
        if (!firstQuestionId) {
            setStartError("Не найдены вопросы упражнения");
            return;
        }
        setStartError(null);
        setStarting(true);
        try {
            const { sessionId } = await wheelApi.startSession(String(user.id));
            navigate(`/exercise/wheel/session/${sessionId}/question/${firstQuestionId}`);
        } catch (err) {
            const message = err instanceof Error ? err.message : "Не удалось начать упражнение";
            setStartError(message);
        } finally {
            setStarting(false);
        }
    };

    return (
        <div style={{ display: "grid", gap: 24 }}>
            <div>
                <h1>Колесо жизненного баланса</h1>
                <p>
                    Это упражнение поможет оценить удовлетворённость различными сферами жизни. Отвечай на вопросы в удобном порядке
                    и добавляй заметки, чтобы зафиксировать свои мысли.
                </p>
            </div>

            {loading && <div>Загрузка данных упражнения…</div>}
            {error && <div role="alert" style={{ color: "crimson" }}>{error}</div>}

            {!loading && !error && (
                <section style={{ display: "grid", gap: 16 }}>
                    <div>
                        <h2>Что тебя ждёт</h2>
                        <ul style={{ paddingLeft: 20, display: "grid", gap: 8 }}>
                            <li>Вопросы по ключевым сферам жизни и возможность пройти их в любом порядке.</li>
                            <li>Подсказки для некоторых вопросов — они помогут взглянуть на ситуацию шире.</li>
                            <li>Фиксация оценки по шкале и заметок, чтобы вернуться к ним позже.</li>
                        </ul>
                    </div>
                    <div>
                        <h3>Сферы, которые мы рассмотрим</h3>
                        <ul style={{ paddingLeft: 20, display: "grid", gap: 4 }}>
                            {dimensions.map(dimension => (
                                <li key={dimension.id}>
                                    <strong>{dimension.name}</strong>{dimension.description ? ` — ${dimension.description}` : ""}
                                </li>
                            ))}
                        </ul>
                    </div>
                    <div>
                        <h3>Список вопросов</h3>
                        <ol style={{ paddingLeft: 20, display: "grid", gap: 4 }}>
                            {questions.map(question => (
                                <li key={question.id}>{question.text}</li>
                            ))}
                        </ol>
                    </div>
                </section>
            )}

            <div style={{ display: "grid", gap: 12, maxWidth: 360 }}>
                {startError && <div role="alert" style={{ color: "crimson" }}>{startError}</div>}
                <button type="button" onClick={onStart} disabled={starting || loading || !!error}>
                    {starting ? "Создаём сессию…" : "Начать упражнение"}
                </button>
                <span style={{ fontSize: 14, color: "#666" }}>
                    После запуска ты сможешь свободно переключаться между вопросами, а ответы сохраняются по кнопке «Сохранить».
                </span>
            </div>
        </div>
    );
}
