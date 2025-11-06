import React, { createContext, useEffect, useState } from "react";

export type User = {
    id: number;
    email: string;
    name: string;
    password?: string;
};

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
        if (raw) setUser(JSON.parse(raw));
    }, []);

    async function login(email: string, password: string) {
        const res = await fetch("http://localhost:5135/users");
        if (!res.ok) {
            throw new Error("Не удалось получить список пользователей");
        }

        const payload = await res.text();
        if (!payload) {
            throw new Error("Сервис авторизации временно недоступен");
        }

        const users: User[] = JSON.parse(payload);
        const found = users.find(u => u.email === email /* && u.password === password */);
        if (!found) throw new Error("User not found or wrong password");
        setUser(found);
        localStorage.setItem("auth_user", JSON.stringify(found));
    }

    async function register(email: string, password: string, name?: string) {
        const res = await fetch("http://localhost:5135/users");
        if (!res.ok) {
            throw new Error("Не удалось получить список пользователей");
        }

        const payload = await res.text();
        if (!payload) {
            throw new Error("Сервис авторизации временно недоступен");
        }

        const users: User[] = JSON.parse(payload);

        if (users.some(u => u.email === email)) {
            throw new Error("Email is already registered");
        }

        const newUser: User = {
            id: Date.now(),
            email,
            name: name ?? email.split("@")[0],
            password,
        };

        await fetch("http://localhost:5135/users", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(newUser),
        });

        const { password: _omit, ...safeUser } = newUser;
        setUser(safeUser);
        localStorage.setItem("auth_user", JSON.stringify(safeUser));
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
