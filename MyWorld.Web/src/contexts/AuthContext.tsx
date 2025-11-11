import React, { createContext, useEffect, useState } from "react";
import { login as loginRequest, register as registerRequest, type AuthUser } from "@/services/auth";

export type User = AuthUser;

export type AuthCtx = {
    user: User | null;
    login: (email: string, password: string) => Promise<void>;
    register: (email: string, password: string, name?: string) => Promise<void>;
    logout: () => void;
};

export const AuthContext = createContext<AuthCtx | null>(null);

export default function AuthProvider({ children }: { children: React.ReactNode }) {
    const [user, setUser] = useState<User | null>(null);

    useEffect(() => {
        const raw = localStorage.getItem("auth_user");
        if (!raw) return;
        try {
            const parsed = JSON.parse(raw) as User;
            setUser(parsed);
        } catch {
            localStorage.removeItem("auth_user");
        }
    }, []);

    async function login(email: string, password: string) {
        const authenticated = await loginRequest({ email, password });
        setUser(authenticated);
        localStorage.setItem("auth_user", JSON.stringify(authenticated));
    }

    async function register(email: string, password: string, name?: string) {
        let firstName: string | undefined;
        let lastName: string | undefined;

        const trimmed = name?.trim();
        if (trimmed) {
            const parts = trimmed.split(/\s+/);
            [firstName] = parts;
            if (parts.length > 1) {
                lastName = parts.slice(1).join(" ");
            }
        }

        const registered = await registerRequest({
            email,
            password,
            firstName,
            lastName,
        });
        setUser(registered);
        localStorage.setItem("auth_user", JSON.stringify(registered));
    }

    function logout() {
        setUser(null);
        localStorage.removeItem("auth_user");
    }

    return (
        <AuthContext.Provider value={{ user, login, register, logout }}>
            {children}
        </AuthContext.Provider>
    );
}
