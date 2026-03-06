import { useContext } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';
import { ThemeContext } from '../context/ThemeContext';

/*
 * Navbar is the top navigation bar shown on every page.
 *
 * It displays:
 * - App logo/name on the left (links to home)
 * - Dark/light mode toggle button (middle/right)
 * - Logout button when authenticated, or Login + Register when not
 *
 * The component uses useContext to access:
 * - AuthContext: to check if user is logged in and call logout()
 * - ThemeContext: to toggle dark mode
 */

export function Navbar() {
  const { isAuthenticated, logout } = useContext(AuthContext);
  const { theme, toggle } = useContext(ThemeContext);
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="bg-white dark:bg-slate-900 border-b border-gray-200 dark:border-slate-700 shadow-sm">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Logo/Brand on the left */}
          <Link
            to="/"
            className="text-xl font-bold text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300"
          >
            🍳 RecipeVault
          </Link>

          {/* Dark mode toggle and authentication buttons on the right */}
          <div className="flex items-center gap-4">
            {/* Dark mode toggle button */}
            <button
              onClick={toggle}
              className="p-2 rounded-lg bg-gray-100 dark:bg-slate-800 hover:bg-gray-200 dark:hover:bg-slate-700 transition"
              title={`Switch to ${theme === 'light' ? 'dark' : 'light'} mode`}
            >
              {theme === 'light' ? '🌙' : '☀️'}
            </button>

            {/* Logout button (only when logged in) */}
            {isAuthenticated && (
              <button
                onClick={handleLogout}
                className="px-4 py-2 rounded-lg bg-red-600 hover:bg-red-700 text-white transition text-sm font-medium"
              >
                Logout
              </button>
            )}

            {/* Login and Register buttons (only when not logged in) */}
            {!isAuthenticated && (
              <div className="hidden md:flex items-center gap-3">
                <button
                  onClick={() => navigate('/login')}
                  className="px-4 py-2 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white transition text-sm font-medium"
                >
                  Login
                </button>
                <button
                  onClick={() => navigate('/register')}
                  className="px-4 py-2 rounded-lg bg-gray-600 hover:bg-gray-700 text-white transition text-sm font-medium"
                >
                  Register
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
}
