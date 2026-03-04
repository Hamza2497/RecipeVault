/*
 * RecipeDetailPage displays a single recipe with full details.
 *
 * What it will do (later):
 * - Get the recipe ID from the URL using useParams()
 * - Fetch the recipe details from GET /api/recipes/{id}
 * - Display recipe name, ingredients, instructions, etc.
 * - Show edit/delete buttons if the current user owns the recipe
 * - Handle navigation to edit page or back to recipe list
 *
 * This route is protected, so only logged-in users can access it.
 *
 * URL example: /recipes/42 would show recipe with ID 42.
 * The ":id" in the route definition is a route parameter.
 *
 * For now, it just shows a heading.
 */

export function RecipeDetailPage() {
  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-8">
        Recipe Detail
      </h1>
      <p className="text-gray-600 dark:text-gray-400">
        Recipe detail coming soon...
      </p>
    </div>
  );
}
