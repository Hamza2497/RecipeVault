import { useContext } from 'react';
import { Navigate } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';

/*
 * ProtectedRoute is a wrapper component that guards routes from unauthorized access.
 *
 * What it does:
 * - Checks if the user is authenticated (has a token)
 * - If authenticated: renders the requested component
 * - If NOT authenticated: redirects to /login
 *
 * How it works:
 * - It's a function that takes a component and returns a new component
 * - In App.jsx, instead of: <Route path="/recipes" element={<RecipeListPage />} />
 * - We use: <Route path="/recipes" element={<ProtectedRoute component={RecipeListPage} />} />
 *
 * Why we need this:
 * - Without this, users could type /recipes in the URL bar and access it even if not logged in
 * - This ensures only authenticated users can view protected pages
 *
 * Usage:
 *   <ProtectedRoute component={RecipeListPage} />
 */

export function ProtectedRoute({ component: Component }) {
  const { isAuthenticated } = useContext(AuthContext);

  // If user is authenticated, show the component; otherwise redirect to login
  return isAuthenticated ? <Component /> : <Navigate to="/login" replace />;
}
