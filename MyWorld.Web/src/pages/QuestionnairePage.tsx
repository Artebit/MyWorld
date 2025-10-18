import QuestionBlock from "@/components/QuestionBlock";
import ResultView from "@/components/ResultView";
import { useQuestionnaire } from "@/hooks/useQuestionnaire";

export default function QuestionnairePage() {
    const { dimensions, questions, sessionId, answers, result, busy, start, changeAnswer, saveAll } = useQuestionnaire();

    return (
        <div style={{ maxWidth: 900, margin: "40px auto", fontFamily: "system-ui, sans-serif" }}>
            <h1>MyWorld — демо</h1>

            <p>API: <code>{import.meta.env.VITE_API_BASE_URL}</code></p>

            <div style={{ marginBottom: 20 }}>
                <button onClick={start} disabled={busy} style={{ padding: "8px 14px" }}>
                    {sessionId ? "Начать заново" : "Начать тест"}
                </button>
                {sessionId && <span style={{ marginLeft: 10 }}>Сессия: {sessionId}</span>}
            </div>

            <h2>Вопросы</h2>
            {!sessionId && <p>Нажми «Начать тест», чтобы отвечать.</p>}

            {sessionId && dimensions.map(d => (
                <QuestionBlock
                    key={d.id}
                    dim={d}
                    questions={questions}
                    answers={answers}
                    onChange={changeAnswer}
                />
            ))}

            {sessionId && (
                <button onClick={saveAll} disabled={busy} style={{ padding: "8px 14px" }}>
                    Сохранить ответы и показать результат
                </button>
            )}

            {result && <ResultView data={result} />}
        </div>
    );
}
