import { useContext } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';
import { ThemeContext } from '../context/ThemeContext';

/*
 * Navbar is the top navigation bar shown on every page.
 *
 * It displays:
 * - App logo/name on the left (links to home)
 * - Navigation links in the middle (only visible when authenticated)
 * - Dark/light mode toggle button
 * - Logout button (only visible when authenticated)
 *
 * Why split the layout?
 * - Tailwind's flexbox utilities let us push items to opposite ends
 * - Using justify-between spreads items: logo left, buttons right
 * - This looks professional without writing custom CSS
 *
 * The component uses useContext to access:
 * - AuthContext: to check if user is logged in and call logout()
 * - ThemeContext: to toggle dark mode
 */

export function Navbar() {
  const { isAuthenticated, logout, user } = useContext(AuthContext);
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

          {/* Navigation links in the middle (only when logged in) */}
          <div className="flex items-center gap-6">
            {isAuthenticated && (
              <>
                <Link to="/" className="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white transition">
                  Recipes
                </Link>
                <Link to="/recipes/new" className="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white transition">
                  Add Recipe
                </Link>
              </>
            )}
          </div>

          {/* Dark mode toggle and logout on the right */}
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
              <div className="flex items-center gap-3">
                <span className="text-sm text-gray-600 dark:text-gray-400">
                  {user?.email || 'User'}
                </span>
                <button
                  onClick={handleLogout}
                  className="px-4 py-2 rounded-lg bg-red-600 hover:bg-red-700 text-white transition text-sm font-medium"
                >
                  Logout
                </button>
              </div>
            )}

            {/* Login and Register buttons (only when not logged in) */}
            {!isAuthenticated && (
              <div className="flex items-center gap-3">
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
