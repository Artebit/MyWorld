import { createContext } from "react";

export type User = {
    id: number;
    email: string;
    name: string;
};

export type AuthCtx = {
    user: User | null;
    login: (email: string, password: string) => Promise<void>;
    logout: () => void;
};

export const AuthContext = createContext<AuthCtx | null>(null);
