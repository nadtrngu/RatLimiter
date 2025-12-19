import axios from 'axios';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
    'X-Admin-Token': import.meta.env.VITE_ADMIN_TOKEN,
  },
});

export default apiClient;
