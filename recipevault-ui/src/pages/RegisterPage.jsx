/*
 * RegisterPage is a placeholder component for the registration form.
 *
 * What it will do (later):
 * - Display a form with email and password inputs
 * - Handle form submission by calling the API register endpoint
 * - Automatically log the user in after successful registration
 * - Redirect to the recipe list
 *
 * For now, it just shows a heading so we can test the router works.
 */

export function RegisterPage() {
  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-50 dark:bg-slate-950">
      <div className="w-full max-w-md">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-8 text-center">
          Create Account
        </h1>
        <p className="text-gray-600 dark:text-gray-400 text-center">
          Registration form coming soon...
        </p>
      </div>
    </div>
  );
}
