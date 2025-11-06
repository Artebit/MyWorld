// src/router.tsx
import { createBrowserRouter } from "react-router-dom";

import AppLayout from "@components/layout/AppLayout";
import AuthGuard from "@components/layout/AuthGuard";

import HomePage from "@pages/HomePage";
import LoginPage from "@pages/Auth/LoginPage";
import RegisterPage from "@pages/Auth/RegisterPage";
import DashboardPage from "@pages/Dashboard/DashboardPage";
import WheelStartPage from "@pages/Exercise/Wheel/WheelStartPage";
import WheelSessionPage from "@pages/Exercise/Wheel/WheelSessionPage";
import WheelResultPage from "@pages/Exercise/Wheel/WheelResultPage";
import HistoryPage from "@pages/History/HistoryPage";
import WheelInfoPage from "@pages/Info/WheelInfoPage";
import AppointmentPage from "@pages/Appointment/AppointmentPage";

const router = createBrowserRouter([
    {
        path: "/",
        element: <AppLayout />,
        children: [
            { index: true, element: <HomePage /> }, 
            { path: "login", element: <LoginPage /> },
            { path: "register", element: <RegisterPage /> },

            {
                element: <AuthGuard />,
                children: [
                    { path: "dashboard", element: <DashboardPage /> },
                    {
                        path: "exercise/wheel",
                        children: [
                            { index: true, element: <WheelStartPage /> },
                            { path: "session/:sessionId/question/:questionId", element: <WheelSessionPage /> },
                            { path: "result/:sessionId", element: <WheelResultPage /> },
                        ],
                    },
                    { path: "history", element: <HistoryPage /> },
                    { path: "info/wheel", element: <WheelInfoPage /> },
                    { path: "appointment", element: <AppointmentPage /> },
                ],
            },
        ],
    },
]);

export default router;
