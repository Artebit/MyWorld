// src/services/auth.ts
import { isAxiosError } from "axios";
import { http, isMock } from "./http";

export type AuthUser = {
    id: string;
    email: string;
    firstName?: string | null;
    lastName?: string | null;
    registeredAt: string;
    lastLoginAt?: string | null;
};

type RegisterBody = {
    email: string;
    password: string;
    firstName?: string | null;
    lastName?: string | null;
};

type LoginBody = {
    email: string;
    password: string;
};

function extractMessage(error: unknown, fallback: string) {
    if (isAxiosError(error)) {
        const payload = error.response?.data as unknown;
        if (payload && typeof payload === "object") {
            const maybeMessage =
                typeof (payload as { message?: unknown }).message === "string"
                    ? (payload as { message?: string }).message
                    : typeof (payload as { title?: unknown }).title === "string"
                      ? (payload as { title?: string }).title
                      : null;
            if (maybeMessage) return maybeMessage;
        }
        if (typeof error.response?.data === "string" && error.response.data.trim().length > 0) {
            return error.response.data;
        }
    }
    if (error instanceof Error) {
        return error.message;
    }
    return fallback;
}

export async function login(body: LoginBody): Promise<AuthUser> {
    if (isMock) {
        await new Promise(r => setTimeout(r, 300));
        return {
            id: "11111111-1111-1111-1111-111111111111",
            email: body.email,
            firstName: "Demo",
            lastName: "User",
            registeredAt: new Date().toISOString(),
            lastLoginAt: new Date().toISOString(),
        };
    }

    try {
        const { data } = await http.post<AuthUser>("/auth/login", body);
        return data;
    } catch (error) {
        throw new Error(extractMessage(error, "Не удалось выполнить вход"));
    }
}

export async function register(body: RegisterBody): Promise<AuthUser> {
    if (isMock) {
        await new Promise(r => setTimeout(r, 300));
        const randomId =
            typeof globalThis.crypto !== "undefined" && typeof globalThis.crypto.randomUUID === "function"
                ? globalThis.crypto.randomUUID()
                : `${Date.now()}-${Math.random().toString(16).slice(2)}`;
        return {
            id: randomId,
            email: body.email,
            firstName: body.firstName ?? null,
            lastName: body.lastName ?? null,
            registeredAt: new Date().toISOString(),
            lastLoginAt: null,
        };
    }

    try {
        const { data } = await http.post<AuthUser>("/auth/register", body);
        return data;
    } catch (error) {
        throw new Error(extractMessage(error, "Не удалось завершить регистрацию"));
    }
}
