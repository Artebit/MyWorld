import type { Dimension, Question } from "@/lib/types";

type Props = {
    dim: Dimension;
    questions: Question[];
    answers: Record<string, number>;
    onChange: (qid: string, val: number) => void;
};

export default function QuestionBlock({ dim, questions, answers, onChange }: Props) {
    const qs = questions.filter(q => q.dimensionId === dim.id).sort((a, b) => a.order - b.order);

    return (
        <div style={{ marginBottom: 24, padding: 12, border: "1px solid #eee", borderRadius: 8 }}>
            <h3 style={{ margin: 0 }}>{dim.name}</h3>
            <small style={{ color: "#666" }}>{dim.description ?? " "}</small>

            {qs.map(q => (
                <div key={q.id} style={{ marginTop: 10 }}>
                    <div>{q.text}</div>
                    <input
                        type="range"
                        min={1}
                        max={10}
                        step={1}
                        value={answers[q.id] ?? 5}
                        onChange={(e) => onChange(q.id, Number(e.target.value))}
                        style={{ width: "100%" }}
                    />
                    <div>Значение: {answers[q.id] ?? 5}</div>
                </div>
            ))}
        </div>
    );
}
