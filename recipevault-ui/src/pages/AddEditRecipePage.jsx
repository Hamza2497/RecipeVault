/*
 * AddEditRecipePage is a form for creating or editing a recipe.
 *
 * What it will do (later):
 * - Check if we're creating (no ID in URL) or editing (ID in URL)
 * - If editing: fetch the recipe from GET /api/recipes/{id} and populate the form
 * - Display form fields for recipe name, ingredients, instructions, etc.
 * - Handle form submission:
 *   - POST /api/recipes for creating a new recipe
 *   - PUT /api/recipes/{id} for updating an existing recipe
 * - Redirect to recipe detail or recipe list after success
 * - Show error message if submission fails
 *
 * This route is protected, so only logged-in users can access it.
 *
 * URL examples:
 * - /recipes/new = create new recipe
 * - /recipes/42/edit = edit recipe with ID 42
 *
 * For now, it just shows a heading.
 */

export function AddEditRecipePage() {
  return (
    <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-8">
        Add/Edit Recipe
      </h1>
      <p className="text-gray-600 dark:text-gray-400">
        Recipe form coming soon...
      </p>
    </div>
  );
}
