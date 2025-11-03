import { Navigate, Outlet, useLocation } from "react-router-dom";
import useAuth from "@/hooks/useAuth";

export default function AuthGuard() {
    const { user } = useAuth();
    const loc = useLocation();

    if (!user) {
        return <Navigate to="/login" replace state={{ from: loc.pathname }} />;
    }
    return <Outlet />;
}
