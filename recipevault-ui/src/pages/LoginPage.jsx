/*
 * LoginPage Component
 *
 * What this component does:
 * This is the login page that users see when they first visit the app. It's a form where they
 * enter their email and password to sign in. When they click "Login", it sends their credentials
 * to the backend server. If the login is successful, they get a token (proof of login) and are
 * taken to the home page. If it fails, they see an error message.
 *
 * Key concepts for beginners:
 * - useState: A React hook that lets components remember things (like form values and errors)
 * - useNavigate: A React hook that lets us change the page after login succeeds
 * - useContext: A hook that lets us access shared data (in this case, the login function)
 * - axios: A tool for sending data to the backend server
 * - Tailwind: A way to style components using class names instead of CSS files
 *
 * Flow:
 * 1. User types their email and password
 * 2. User clicks the Login button
 * 3. We send their email and password to the backend
 * 4. Backend checks if they're correct and sends back a token
 * 5. We save the token and user data using the login() function
 * 6. We navigate to the home page
 */

import { useState, useContext } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';
import api from '../api/axios';

export function LoginPage() {
  const navigate = useNavigate();
  const { login } = useContext(AuthContext);

  // Form state: stores what the user types in the input fields
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  // UI state: tracks if we're waiting for the server and any error messages
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  // Handle form submission (when user clicks Login button)
  const handleSubmit = async (e) => {
    e.preventDefault(); // Prevent page refresh
    setError(''); // Clear any previous errors
    setIsLoading(true); // Show loading state

    try {
      // Send login request to backend
      const response = await api.post('/auth/login', {
        email,
        password,
      });

      // Extract token and user from response
      const { token, user } = response.data;

      // Save to AuthContext and localStorage
      login(token, user);

      // Redirect to home page
      navigate('/');
    } catch (err) {
      // Show error message if login fails
      const errorMessage = err.response?.data?.message || 'Login failed. Please try again.';
      setError(errorMessage);
    } finally {
      setIsLoading(false); // Hide loading state
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-50 dark:bg-slate-950 px-4">
      <div className="w-full max-w-md">
        {/* Card container with shadow and rounded corners */}
        <div className="bg-white dark:bg-slate-900 rounded-lg shadow-lg p-8">
          {/* Heading */}
          <h1 className="text-3xl font-bold text-center text-gray-900 dark:text-white mb-2">
            Welcome Back
          </h1>
          <p className="text-center text-gray-600 dark:text-gray-400 mb-8">
            Sign in to your RecipeVault account
          </p>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Email input */}
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Email
              </label>
              <input
                id="email"
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@example.com"
                className="w-full px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
              />
            </div>

            {/* Password input */}
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Password
              </label>
              <input
                id="password"
                type="password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                className="w-full px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
              />
            </div>

            {/* Error message display */}
            {error && (
              <div className="p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
                <p className="text-sm text-red-800 dark:text-red-300">{error}</p>
              </div>
            )}

            {/* Submit button */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full py-2 px-4 bg-indigo-600 hover:bg-indigo-700 dark:bg-indigo-500 dark:hover:bg-indigo-600 text-white font-medium rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isLoading ? 'Logging in...' : 'Login'}
            </button>
          </form>

          {/* Divider */}
          <div className="mt-6 pt-6 border-t border-gray-300 dark:border-slate-700">
            {/* Link to register page */}
            <p className="text-center text-gray-600 dark:text-gray-400">
              Don't have an account?{' '}
              <Link
                to="/register"
                className="font-medium text-indigo-600 hover:text-indigo-700 dark:text-indigo-400 dark:hover:text-indigo-300 transition"
              >
                Sign up here
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
