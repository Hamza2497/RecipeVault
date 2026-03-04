/*
 * RecipeListPage displays all recipes the user has access to.
 *
 * What it will do (later):
 * - Fetch recipes from GET /api/recipes on component mount
 * - Display recipes in a grid or list
 * - Show loading spinner while fetching
 * - Show error message if API call fails
 * - Link each recipe to its detail page
 * - Show "Create Recipe" button
 *
 * This route is protected, so only logged-in users can access it.
 * If someone tries to visit /recipes without logging in, ProtectedRoute redirects to /login.
 *
 * For now, it just shows a heading.
 */

export function RecipeListPage() {
  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-8">
        My Recipes
      </h1>
      <p className="text-gray-600 dark:text-gray-400">
        Recipe list coming soon...
      </p>
    </div>
  );
}
