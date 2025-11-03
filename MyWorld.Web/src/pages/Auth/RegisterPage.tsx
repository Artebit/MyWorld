import { useState } from "react";
import useAuth from "@/hooks/useAuth";
import { useNavigate, Link } from "react-router-dom";

export default function RegisterPage() {
    const { register } = useAuth();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [err, setErr] = useState<string | null>(null);
    const nav = useNavigate();

    const onSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErr(null);
        try {
            await register(email, password);
            nav("/dashboard", { replace: true });
        } catch (e: unknown) {
            if (e instanceof Error) setErr(e.message);
            else setErr("register error");
        }

    };

    return (
        <div style={{ maxWidth: 480, margin: "40px auto" }}>
            <h1>Регистрация</h1>
            <form onSubmit={onSubmit} style={{ display: "grid", gap: 12 }}>
                <input placeholder="Email" value={email} onChange={e => setEmail(e.target.value)} />
                <input placeholder="Пароль" type="password" value={password} onChange={e => setPassword(e.target.value)} />
                <button type="submit">Создать аккаунт</button>
                {err && <div style={{ color: "crimson" }}>{err}</div>}
            </form>
            <p>Уже есть аккаунт? <Link to="/login">Войти</Link></p>
        </div>
    );
}
