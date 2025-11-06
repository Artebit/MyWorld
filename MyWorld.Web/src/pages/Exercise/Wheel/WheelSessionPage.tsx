import { useEffect, useMemo, useState, type CSSProperties } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useWheelSession } from "@/hooks/useWheelSession";

const containerStyle: CSSProperties = {
    display: "grid",
    gap: 24,
};

const navigationStyle: CSSProperties = {
    display: "flex",
    flexWrap: "wrap",
    gap: 8,
};

const hintStyle: CSSProperties = {
    background: "#f3f4ff",
    border: "1px solid #cfd2ff",
    borderRadius: 8,
    padding: "12px 16px",
};

export default function WheelSessionPage() {
    const { sessionId, questionId } = useParams<{ sessionId: string; questionId?: string }>();
    const navigate = useNavigate();
    const {
        loading,
        error,
        questions,
        dimensionsMap,
        answers,
        setAnswerNote,
        setAnswerValue,
        saveAnswer,
        completeSession,
        completeBusy,
        completeError,
    } = useWheelSession(sessionId);
    const [hintOpen, setHintOpen] = useState(false);
    const [saveMessage, setSaveMessage] = useState<string | null>(null);

    useEffect(() => {
        setHintOpen(false);
        setSaveMessage(null);
    }, [questionId]);

    useEffect(() => {
        if (loading) return;
        if (!questions.length) return;
        if (!questionId || !questions.some(q => q.id === questionId)) {
            navigate(`/exercise/wheel/session/${sessionId}/question/${questions[0].id}`, { replace: true });
        }
    }, [loading, questions, questionId, navigate, sessionId]);

    const currentQuestion = useMemo(
        () => questions.find(q => q.id === questionId),
        [questions, questionId],
    );

    const currentAnswer = currentQuestion ? answers[currentQuestion.id] : undefined;

    const currentDimension = currentQuestion ? dimensionsMap[currentQuestion.dimensionId] : undefined;

    const currentIndex = useMemo(
        () => (currentQuestion ? questions.findIndex(q => q.id === currentQuestion.id) : -1),
        [currentQuestion, questions],
    );

    const prevQuestion = currentIndex > 0 ? questions[currentIndex - 1] : null;
    const nextQuestion = currentIndex >= 0 && currentIndex < questions.length - 1 ? questions[currentIndex + 1] : null;

    const onSave = async () => {
        if (!currentQuestion) return;
        setSaveMessage(null);
        try {
            await saveAnswer(currentQuestion.id);
            setSaveMessage("Ответ сохранён");
        } catch (err) {
            if (err instanceof Error) {
                setSaveMessage(err.message);
            } else {
                setSaveMessage("Не удалось сохранить ответ");
            }
        }
    };

    const onComplete = async () => {
        if (!sessionId) return;
        try {
            await completeSession();
            navigate(`/exercise/wheel/result/${sessionId}`);
        } catch (err) {
            // Ошибка уже отображается через completeError
        }
    };

    if (!sessionId) {
        return <div>Не удалось определить идентификатор сессии.</div>;
    }

    if (loading) {
        return <div>Загружаем вопросы…</div>;
    }

    if (error) {
        return (
            <div role="alert" style={{ color: "crimson" }}>
                Не удалось загрузить упражнение: {error}
            </div>
        );
    }

    if (!currentQuestion) {
        return <div>Вопрос не найден.</div>;
    }

    const answerValue = typeof currentAnswer?.value === "number" ? currentAnswer.value : 5;
    const isValueSet = typeof currentAnswer?.value === "number";

    return (
        <div style={containerStyle}>
            <header style={{ display: "grid", gap: 8 }}>
                <h1>Вопрос {currentIndex + 1} из {questions.length}</h1>
                <p style={{ margin: 0 }}>{currentQuestion.text}</p>
                {currentDimension && (
                    <span style={{ fontSize: 14, color: "#555" }}>
                        Сфера: {currentDimension.name}{currentDimension.description ? ` — ${currentDimension.description}` : ""}
                    </span>
                )}
            </header>

            <nav style={navigationStyle}>
                {questions.map((question, index) => {
                    const state = answers[question.id];
                    const isActive = question.id === currentQuestion.id;
                    const isAnswered =
                        state?.saved ||
                        (!!state?.note && state.note.trim().length > 0) ||
                        typeof state?.value === "number";
                    return (
                        <Link
                            key={question.id}
                            to={`/exercise/wheel/session/${sessionId}/question/${question.id}`}
                            style={{
                                padding: "6px 10px",
                                borderRadius: 6,
                                border: "1px solid",
                                borderColor: isActive ? "#646cff" : isAnswered ? "#4caf50" : "#ccc",
                                background: isActive ? "#e8e9ff" : isAnswered ? "#f3fff3" : "#fff",
                                color: "inherit",
                                fontWeight: isActive ? 600 : 500,
                                textDecoration: "none",
                            }}
                        >
                            {index + 1}
                        </Link>
                    );
                })}
            </nav>

            {currentQuestion.hint && (
                <div style={{ display: "grid", gap: 8 }}>
                    <button type="button" onClick={() => setHintOpen(value => !value)}>
                        {hintOpen ? "Скрыть подсказку" : "Показать подсказку"}
                    </button>
                    {hintOpen && <div style={hintStyle}>{currentQuestion.hint}</div>}
                </div>
            )}

            <section style={{ display: "grid", gap: 16 }}>
                <div style={{ display: "grid", gap: 8 }}>
                    <label style={{ fontWeight: 600 }} htmlFor="question-score">
                        Оцени текущую удовлетворённость по шкале от 0 до 10
                    </label>
                    <input
                        id="question-score"
                        type="range"
                        min={0}
                        max={10}
                        value={answerValue}
                        onChange={event => setAnswerValue(currentQuestion.id, Number(event.target.value))}
                    />
                    <span style={{ fontSize: 14, color: "#555" }}>
                        {isValueSet ? `Текущая оценка: ${currentAnswer?.value}` : "Пока без оценки"}
                    </span>
                </div>

                <div style={{ display: "grid", gap: 8 }}>
                    <label style={{ fontWeight: 600 }} htmlFor="question-note">
                        Запиши мысли и наблюдения
                    </label>
                    <textarea
                        id="question-note"
                        rows={6}
                        value={currentAnswer?.note ?? ""}
                        onChange={event => setAnswerNote(currentQuestion.id, event.target.value)}
                        placeholder="Что происходит в этой сфере? Что хочется изменить?"
                        style={{ padding: 12, borderRadius: 8, border: "1px solid #ccc", resize: "vertical" }}
                    />
                </div>

                {currentAnswer?.error && (
                    <div role="alert" style={{ color: "crimson" }}>{currentAnswer.error}</div>
                )}

                <div style={{ display: "flex", gap: 12, flexWrap: "wrap" }}>
                    <button type="button" onClick={onSave} disabled={currentAnswer?.saving}>
                        {currentAnswer?.saving ? "Сохраняем…" : "Сохранить ответ"}
                    </button>
                    <button type="button" disabled={!prevQuestion} onClick={() => prevQuestion && navigate(`/exercise/wheel/session/${sessionId}/question/${prevQuestion.id}`)}>
                        Назад
                    </button>
                    <button type="button" disabled={!nextQuestion} onClick={() => nextQuestion && navigate(`/exercise/wheel/session/${sessionId}/question/${nextQuestion.id}`)}>
                        Вперёд
                    </button>
                </div>

                {saveMessage && <div style={{ color: currentAnswer?.error ? "crimson" : "#2e7d32" }}>{saveMessage}</div>}
            </section>

            <footer style={{ display: "grid", gap: 8 }}>
                {completeError && <div role="alert" style={{ color: "crimson" }}>{completeError}</div>}
                <button type="button" onClick={onComplete} disabled={completeBusy}>
                    {completeBusy ? "Завершаем…" : "Завершить упражнение"}
                </button>
            </footer>
        </div>
    );
}
