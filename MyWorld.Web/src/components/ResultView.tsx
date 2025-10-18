import type { ResultItem } from "@/lib/types";

export default function ResultView({ data }: { data: ResultItem[] }) {
    return (
        <>
            <h2>Результат</h2>
            <pre style={{ background: "#f6f8fa", padding: 12, borderRadius: 8 }}>
                {JSON.stringify(data, null, 2)}
            </pre>
        </>
    );
}
