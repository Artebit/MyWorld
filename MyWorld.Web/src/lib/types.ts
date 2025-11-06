export type Dimension = {
    id: string;
    name: string;
    description: string | null;
};

export type Question = {
    id: string;
    text: string;
    order: number;
    dimensionId: string;
    hint?: string | null;
};

export type SessionStartResponse = {
    sessionId: string;
};

// подстрой под реальный ответ твоего бэка, если нужно
export type ResultItem = {
    dimensionId: string;
    value: number;
};
