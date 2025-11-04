import { useState } from "react";
import { useLocation, useNavigate, Link } from "react-router-dom";
import useAuth from "@hooks/useAuth";

export default function LoginPage() {
    const { login } = useAuth();
    const nav = useNavigate();
    const loc = useLocation() as unknown as { state?: { from?: string } };
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [err, setErr] = useState<string | null>(null);

    const onSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErr(null);
        try {
            await login(email, password);
            const back = loc.state?.from ?? "/dashboard";
            nav(back, { replace: true });
        } catch (e: unknown) {
            setErr(e instanceof Error ? e.message : "Login error");
        }
    };

    return (
        <div>
            <h1>Вход</h1>
            {err && <div style={{ color: "crimson" }}>{err}</div>}
            <form onSubmit={onSubmit} style={{ display: "grid", gap: 12, maxWidth: 360 }}>
                <input placeholder="Email" value={email} onChange={e => setEmail(e.target.value)} />
                <input type="password" placeholder="Пароль" value={password} onChange={e => setPassword(e.target.value)} />
                <button type="submit">Войти</button>
            </form>
            <p>Нет аккаунта? <Link to="/register">Регистрация</Link></p>
        </div>
    );
}
