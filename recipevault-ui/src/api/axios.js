import axios from 'axios';

/*
 * This creates an Axios instance configured to communicate with our ASP.NET Core API.
 * The baseURL is set to the API's URL on localhost:5159.
 *
 * The interceptor automatically adds the JWT token from localStorage to every API request
 * as an Authorization header. This allows protected endpoints to verify the user is logged in.
 *
 * Example: When you call api.get('/recipes'), it becomes a request to:
 * http://localhost:5159/api/recipes with "Authorization: Bearer {token}"
 */

const api = axios.create({
  baseURL: 'http://localhost:5159/api',
});

// Add a request interceptor to attach the JWT token to every request
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export default api;
