// src/services/auth.ts
import { http, isMock } from "./http";

export async function login(body: { email: string; password: string }) {
    if (isMock) {
        await new Promise(r => setTimeout(r, 300));
        return { token: "FAKE", user: { id: "u1", email: body.email } };
    }
    const { data } = await http.post("/auth/login", body);
    return data;
}
