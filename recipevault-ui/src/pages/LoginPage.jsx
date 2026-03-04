/*
 * LoginPage is a placeholder component for the login form.
 *
 * What it will do (later):
 * - Display a form with email and password inputs
 * - Handle form submission by calling the API login endpoint
 * - Store the returned JWT token and user data using AuthContext
 * - Redirect to the recipe list (/recipes) on successful login
 *
 * For now, it just shows a heading so we can test the router works.
 */

export function LoginPage() {
  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-50 dark:bg-slate-950">
      <div className="w-full max-w-md">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-8 text-center">
          Login to RecipeVault
        </h1>
        <p className="text-gray-600 dark:text-gray-400 text-center">
          Login form coming soon...
        </p>
      </div>
    </div>
  );
}
