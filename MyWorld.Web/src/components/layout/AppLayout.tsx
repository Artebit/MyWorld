import { Outlet, Link } from "react-router-dom";
import useAuth from "@hooks/useAuth";

export default function AppLayout() {
    const { user, logout } = useAuth();

    return (
        <div style={{ maxWidth: 900, margin: "24px auto" }}>
            <nav style={{ display: "flex", gap: 12, marginBottom: 16 }}>
                <Link to="/">Home</Link>
                {!user && <>
                    <Link to="/login">Login</Link>
                    <Link to="/register">Register</Link>
                </>}
                {user && <>
                    <Link to="/dashboard">Dashboard</Link>
                    <Link to="/exercise/wheel">Wheel</Link>
                    <Link to="/history">History</Link>
                    <Link to="/appointment">Appointment</Link>
                    <button onClick={logout}>logout</button>
                </>}
            </nav>
            <Outlet />
        </div>
    );
}
