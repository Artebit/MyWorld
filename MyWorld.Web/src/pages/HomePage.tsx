// src/pages/HomePage.tsx
import { Link, useNavigate } from "react-router-dom";
import useAuth from "@/hooks/useAuth";

export default function HomePage() {
    const navigate = useNavigate();
    const { user, logout } = useAuth();      // <- теперь это тип AuthCtx
    const isAuthenticated = !!user;          // <- локально считаем флаг

    return (
        <div style={{ maxWidth: 900, margin: "40px auto", fontFamily: "system-ui, sans-serif" }}>
            <h1>MyWorld — домашняя</h1>

            {isAuthenticated ? (
                <>
                    <p>Вы вошли как <b>{user?.email}</b>.</p>
                    <div style={{ display: "flex", gap: 12, flexWrap: "wrap" }}>
                        <Link to="/dashboard">Перейти в кабинет</Link>
                        <Link to="/exercise/wheel">Начать «Колесо баланса»</Link>
                        <Link to="/history">История результатов</Link>
                        <Link to="/appointment">Запись на консультацию</Link>
                        <button onClick={() => { logout(); navigate("/"); }}>Выйти</button>
                    </div>
                </>
            ) : (
                <>
                    <p>Добро пожаловать! Войдите или зарегистрируйтесь, чтобы пройти упражнение.</p>
                    <div style={{ display: "flex", gap: 12 }}>
                        <Link to="/login">Войти</Link>
                        <Link to="/register">Зарегистрироваться</Link>
                        <Link to="/exercise/wheel">Начать колесо баланса</Link>
                    </div>
                </>
            )}
        </div>
    );
}
