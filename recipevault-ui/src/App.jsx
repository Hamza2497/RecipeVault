import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Navbar } from './components/Navbar';
import { ProtectedRoute } from './components/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { RecipeListPage } from './pages/RecipeListPage';
import { RecipeDetailPage } from './pages/RecipeDetailPage';
import { AddEditRecipePage } from './pages/AddEditRecipePage';

/*
 * App.jsx is the root component that sets up React Router.
 *
 * What it does:
 * 1. Wraps the app in a Router so we can navigate between pages
 * 2. Displays the Navbar on every page (outside the Routes)
 * 3. Defines all routes:
 *    - Public routes: /login, /register (anyone can access)
 *    - Protected routes: / (recipes), /recipes/:id, /recipes/new, /recipes/:id/edit
 *      (only authenticated users can access, others are redirected to /login)
 * 4. Default route / redirects to /recipes (recipe list)
 *
 * Route structure:
 * - Routes are defined with path and element
 * - :id is a route parameter (can access with useParams())
 * - ProtectedRoute wraps components that need authentication
 *
 * Navigation flow:
 * - User visits /login or /register to create account
 * - After login, user is redirected to / (recipe list)
 * - User clicks "Add Recipe" -> navigates to /recipes/new
 * - User clicks on a recipe -> navigates to /recipes/:id
 * - User clicks "Edit" -> navigates to /recipes/:id/edit
 */

function App() {
  return (
    <Router>
      <div className="flex flex-col min-h-screen bg-white dark:bg-slate-950 text-gray-900 dark:text-white">
        <Navbar />
        <main className="flex-grow">
          <Routes>
            {/* Public routes */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            {/* Protected routes */}
            <Route path="/" element={<ProtectedRoute component={RecipeListPage} />} />
            <Route path="/recipes/:id" element={<ProtectedRoute component={RecipeDetailPage} />} />
            <Route path="/recipes/new" element={<ProtectedRoute component={AddEditRecipePage} />} />
            <Route path="/recipes/:id/edit" element={<ProtectedRoute component={AddEditRecipePage} />} />

            {/* Catch-all: redirect unknown routes to home */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

export default App;
