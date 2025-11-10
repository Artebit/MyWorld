import React, { createContext, useEffect, useState } from "react";
import { login as loginRequest, register as registerRequest, type AuthUser } from "@/Services/auth";

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

        if (name && name.trim().length > 0) {
            const parts = name.trim().split(/\s+/);
            firstName = parts[0];
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
