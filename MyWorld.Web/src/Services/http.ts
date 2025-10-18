import axios from "axios";

const BASE = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5135/api";

export const http = axios.create({ baseURL: BASE });

// Если сервер требует идентификатор пользователя в заголовке — раскомментируй:
// http.interceptors.request.use((config) => {
//   const uid = import.meta.env.VITE_DEMO_USER_ID;
//   if (uid) config.headers['X-UserId'] = uid;
//   return config;
// });
