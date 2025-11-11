import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { wheelApi } from "@/Services/wheel";
import type { Dimension, ResultItem } from "@/lib/types";

export default function WheelResultPage() {
    const { sessionId } = useParams<{ sessionId: string }>();
    const [result, setResult] = useState<ResultItem[] | null>(null);
    const [dimensions, setDimensions] = useState<Dimension[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!sessionId) return;
        let active = true;
        setLoading(true);6
        setError(null);
        (async () => {
            try {
                const [items, dims] = await Promise.all([
                    wheelApi.getResult(sessionId),
                    wheelApi.getDimensions(),
                ]);
                if (!active) return;
                setResult(items);
                setDimensions(dims);
            } catch (err) {
                if (!active) return;
                const message = err instanceof Error ? err.message : "Не удалось получить результаты";
                setError(message);
            } finally {
                if (active) setLoading(false);
            }
        })();

        return () => {
            active = false;
        };
    }, [sessionId]);

    if (!sessionId) {
        return <div>Не указан идентификатор сессии.</div>;
    }

    if (loading) {
        return <div>Готовим результаты…</div>;
    }

    if (error) {
        return (
            <div role="alert" style={{ color: "crimson" }}>
                Ошибка: {error}
            </div>
        );
    }

    if (!result || result.length === 0) {
        return (
            <div>
                <h1>Результат недоступен</h1>
                <p>Мы не нашли данных для этой сессии. Попробуй пройти упражнение ещё раз.</p>
                <Link to="/exercise/wheel">Вернуться к упражнениям</Link>
            </div>
        );
    }

    const dimensionsMap = dimensions.reduce<Record<string, Dimension>>((acc, dimension) => {
        acc[dimension.id] = dimension;
        return acc;
    }, {});

    return (
        <div style={{ display: "grid", gap: 24 }}>
            <header>
                <h1>Результат упражнения</h1>
                <p style={{ maxWidth: 620 }}>
                    Ниже ты видишь оценки по сферам жизни, которые были зафиксированы во время упражнения. Обсуди их со своим коучем
                    или вернись к заметкам, чтобы наметить дальнейшие шаги.
                </p>
            </header>

            <table style={{ borderCollapse: "collapse", maxWidth: 520 }}>
                <thead>
                    <tr>
                        <th style={{ textAlign: "left", borderBottom: "1px solid #ccc", padding: "8px 12px" }}>Сфера</th>
                        <th style={{ textAlign: "left", borderBottom: "1px solid #ccc", padding: "8px 12px" }}>Оценка</th>
                    </tr>
                </thead>
                <tbody>
                    {result.map(item => {
                        const dimension = dimensionsMap[item.dimensionId];
                        return (
                            <tr key={item.dimensionId}>
                                <td style={{ padding: "8px 12px", borderBottom: "1px solid #eee" }}>
                                    {dimension ? dimension.name : item.dimensionId}
                                </td>
                                <td style={{ padding: "8px 12px", borderBottom: "1px solid #eee" }}>{item.value}</td>
                            </tr>
                        );
                    })}
                </tbody>
            </table>

            <div style={{ display: "flex", gap: 16, flexWrap: "wrap" }}>
                <Link to="/dashboard">Перейти в дашборд</Link>
                <Link to="/exercise/wheel">Начать новое упражнение</Link>
            </div>
        </div>
    );
}
