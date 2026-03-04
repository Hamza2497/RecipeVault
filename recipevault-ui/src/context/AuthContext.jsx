import { createContext, useState, useCallback, useEffect } from 'react';

/*
 * AuthContext is a React Context that manages user authentication state globally.
 *
 * What it does:
 * - Stores the current user object and their JWT token
 * - Provides login(token, user) function to set user data and save token to localStorage
 * - Provides logout() function to clear user data and remove token from localStorage
 * - On first load, it checks if a token exists in localStorage and restores the user session
 *
 * Why Context instead of local state?
 * - If you stored auth state in App.jsx, every child component would need to pass it down
 * - Context lets ANY component access auth state without "prop drilling"
 *
 * Usage in a component:
 *   const { user, isAuthenticated, login, logout } = useContext(AuthContext);
 */

export const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(null);

  // Check if user was already logged in (token in localStorage)
  useEffect(() => {
    const savedToken = localStorage.getItem('token');
    if (savedToken) {
      setToken(savedToken);
      // In a real app, you might fetch user details from API here
      const savedUser = localStorage.getItem('user');
      if (savedUser) {
        setUser(JSON.parse(savedUser));
      }
    }
  }, []);

  // Login: save token and user to state and localStorage
  const login = useCallback((token, user) => {
    setToken(token);
    setUser(user);
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(user));
  }, []);

  // Logout: clear state and localStorage
  const logout = useCallback(() => {
    setToken(null);
    setUser(null);
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  }, []);

  const value = {
    user,
    token,
    isAuthenticated: !!token,
    login,
    logout,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}
