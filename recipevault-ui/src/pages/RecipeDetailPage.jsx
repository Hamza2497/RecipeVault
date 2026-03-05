import { useState, useEffect, useCallback, useContext } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import { AuthContext } from '../context/AuthContext';
import api from '../api/axios';

// API base URL for unauthenticated requests
const API_BASE_URL = import.meta.env.VITE_API_URL;

/*
 * RecipeDetailPage displays a single recipe with full details, AI features, and management options.
 *
 * Features:
 * - Fetches recipe by ID from GET /api/recipes/:id
 * - Displays all recipe fields: name, description, ingredients, instructions, cuisine type, prep/cook time, servings, image
 * - Status dropdown (favourite, to-try, made-before, none) that calls PATCH /api/recipes/:id/status
 * - Public/private toggle that calls PATCH /api/recipes/:id/visibility
 * - Edit button linking to /recipes/:id/edit
 * - Delete button with confirmation dialog that calls DELETE /api/recipes/:id
 * - Back button to return to /
 * - AI panel with three collapsible sections:
 *   - Generate New Recipe: create recipes with constraints
 *   - Substitute Ingredient: find substitutes for ingredients
 *   - Tweak Recipe: modify current recipe with AI suggestions
 * - Full dark mode support
 * - Indigo accent color for UI elements
 */

export function RecipeDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user, token } = useContext(AuthContext);
  const isLoggedIn = !!token;

  // Recipe state
  const [recipe, setRecipe] = useState(null);
  const [isLoadingRecipe, setIsLoadingRecipe] = useState(true);
  const [recipeError, setRecipeError] = useState('');

  // Action states
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  // AI panel state
  const [expandedAISection, setExpandedAISection] = useState(null);
  const [aiLoading, setAiLoading] = useState({});
  const [aiError, setAiError] = useState({});

  // Generate Recipe state
  const [generateName, setGenerateName] = useState('');
  const [generateConstraints, setGenerateConstraints] = useState('');
  const [generatedRecipe, setGeneratedRecipe] = useState(null);

  // Substitute Ingredient state
  const [substituteIngredient, setSubstituteIngredient] = useState('');
  const [substituteReason, setSubstituteReason] = useState('');
  const [substitutionResults, setSubstitutionResults] = useState(null);

  // Tweak Recipe state
  const [tweakRequest, setTweakRequest] = useState('');
  const [tweakedRecipe, setTweakedRecipe] = useState(null);

  // Fetch recipe details
  const fetchRecipe = useCallback(async () => {
    setIsLoadingRecipe(true);
    setRecipeError('');

    try {
      // Use plain axios with full URL for unauthenticated users, configured api for authenticated users
      if (isLoggedIn) {
        const response = await api.get(`/recipes/${id}`);
        setRecipe(response.data);
      } else {
        const response = await axios.get(`${API_BASE_URL}/recipes/public/${id}`);
        setRecipe(response.data);
      }
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to load recipe. Please try again.';
      setRecipeError(errorMessage);
    } finally {
      setIsLoadingRecipe(false);
    }
  }, [id, isLoggedIn]);

  useEffect(() => {
    fetchRecipe();
  }, [fetchRecipe]);

  // Update recipe status
  const handleStatusChange = async (newStatus) => {
    try {
      const response = await api.patch(`/recipes/${id}/status`, { status: newStatus });
      setRecipe(response.data);
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to update status.';
      setRecipeError(errorMessage);
    }
  };

  // Toggle recipe visibility
  const handleVisibilityToggle = async () => {
    try {
      const newVisibility = recipe.isPublic ? 'private' : 'public';
      const response = await api.patch(`/recipes/${id}/visibility`, { isPublic: !recipe.isPublic });
      setRecipe(response.data);
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to update visibility.';
      setRecipeError(errorMessage);
    }
  };

  // Delete recipe
  const handleDelete = async () => {
    setIsDeleting(true);
    try {
      await api.delete(`/recipes/${id}`);
      navigate('/');
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to delete recipe.';
      setRecipeError(errorMessage);
      setIsDeleting(false);
      setShowDeleteConfirm(false);
    }
  };

  // Generate new recipe
  const handleGenerateRecipe = async () => {
    if (!generateName.trim()) {
      setAiError(prev => ({ ...prev, generate: 'Please enter a recipe name.' }));
      return;
    }

    setAiLoading(prev => ({ ...prev, generate: true }));
    setAiError(prev => ({ ...prev, generate: '' }));

    try {
      const response = await api.post('/ai/generate', {
        recipeName: generateName,
        constraints: generateConstraints,
      });
      setGeneratedRecipe(response.data);
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to generate recipe.';
      setAiError(prev => ({ ...prev, generate: errorMessage }));
    } finally {
      setAiLoading(prev => ({ ...prev, generate: false }));
    }
  };

  // Save generated recipe
  const handleSaveGeneratedRecipe = async () => {
    try {
      await api.post('/recipes', generatedRecipe);
      setGeneratedRecipe(null);
      setGenerateName('');
      setGenerateConstraints('');
      setExpandedAISection(null);
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to save recipe.';
      setAiError(prev => ({ ...prev, generate: errorMessage }));
    }
  };

  // Substitute ingredient
  const handleSubstituteIngredient = async () => {
    if (!substituteIngredient.trim()) {
      setAiError(prev => ({ ...prev, substitute: 'Please enter an ingredient.' }));
      return;
    }

    setAiLoading(prev => ({ ...prev, substitute: true }));
    setAiError(prev => ({ ...prev, substitute: '' }));

    try {
      const response = await api.post(`/ai/substitute/${encodeURIComponent(substituteIngredient)}`, {
        reason: substituteReason,
      });
      setSubstitutionResults(response.data);
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to find substitutions.';
      setAiError(prev => ({ ...prev, substitute: errorMessage }));
    } finally {
      setAiLoading(prev => ({ ...prev, substitute: false }));
    }
  };

  // Tweak recipe
  const handleTweakRecipe = async () => {
    if (!tweakRequest.trim()) {
      setAiError(prev => ({ ...prev, tweak: 'Please enter a tweak request.' }));
      return;
    }

    setAiLoading(prev => ({ ...prev, tweak: true }));
    setAiError(prev => ({ ...prev, tweak: '' }));

    try {
      const currentRecipe = {
        name: recipe.name,
        ingredients: Array.isArray(recipe.ingredients)
          ? recipe.ingredients
          : recipe.ingredients?.split(',').map(i => i.trim()) ?? [],
        instructions: Array.isArray(recipe.instructions)
          ? recipe.instructions
          : recipe.instructions?.split('\n').map(i => i.trim()).filter(Boolean) ?? [],
        cuisineType: recipe.cuisineType,
        prepTimeMinutes: recipe.prepTimeMinutes,
        cookTimeMinutes: recipe.cookTimeMinutes,
        servings: recipe.servings
      };

      const response = await api.post('/ai/tweak', {
        currentRecipe: currentRecipe,
        tweakRequest: tweakRequest,
      });
      setTweakedRecipe(response.data);
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to tweak recipe.';
      setAiError(prev => ({ ...prev, tweak: errorMessage }));
    } finally {
      setAiLoading(prev => ({ ...prev, tweak: false }));
    }
  };

  // Apply tweaked recipe
  const handleApplyTweaks = async () => {
    try {
      const updatePayload = {
        name: tweakedRecipe.name,
        ingredients: Array.isArray(tweakedRecipe.ingredients)
          ? tweakedRecipe.ingredients.join(', ')
          : tweakedRecipe.ingredients,
        instructions: Array.isArray(tweakedRecipe.instructions)
          ? tweakedRecipe.instructions.join('\n')
          : tweakedRecipe.instructions,
        cuisineType: tweakedRecipe.cuisineType,
        prepTimeMinutes: tweakedRecipe.prepTimeMinutes,
        cookTimeMinutes: tweakedRecipe.cookTimeMinutes,
        servings: tweakedRecipe.servings
      };
      await api.put(`/recipes/${id}`, updatePayload);
      setRecipe(tweakedRecipe);
      setTweakedRecipe(null);
      setTweakRequest('');
      setExpandedAISection(null);
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to apply tweaks.';
      setAiError(prev => ({ ...prev, tweak: errorMessage }));
    }
  };

  // Check if user owns the recipe
  const isOwnRecipe = isLoggedIn && user && recipe && recipe.userId === user.id;

  // Loading state
  if (isLoadingRecipe) {
    return (
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="flex items-center justify-center py-16">
          <div className="flex flex-col items-center gap-4">
            <div className="w-8 h-8 border-4 border-gray-300 dark:border-slate-700 border-t-indigo-600 dark:border-t-indigo-500 rounded-full animate-spin"></div>
            <p className="text-gray-600 dark:text-gray-400">Loading recipe...</p>
          </div>
        </div>
      </div>
    );
  }

  // Error state
  if (recipeError && !recipe) {
    return (
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <button
          onClick={() => navigate('/')}
          className="mb-6 px-4 py-2 text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 dark:hover:text-indigo-300 font-medium transition"
        >
          ← Back
        </button>
        <div className="p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
          <p className="text-sm text-red-800 dark:text-red-300">{recipeError}</p>
        </div>
      </div>
    );
  }

  if (!recipe) {
    return (
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <button
          onClick={() => navigate('/')}
          className="mb-6 px-4 py-2 text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 dark:hover:text-indigo-300 font-medium transition"
        >
          ← Back
        </button>
        <p className="text-gray-600 dark:text-gray-400">Recipe not found.</p>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Back button */}
      <button
        onClick={() => navigate('/')}
        className="mb-6 px-4 py-2 text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 dark:hover:text-indigo-300 font-medium transition"
      >
        ← Back
      </button>

      {/* Error message */}
      {recipeError && (
        <div className="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
          <p className="text-sm text-red-800 dark:text-red-300">{recipeError}</p>
        </div>
      )}

      {/* Delete confirmation dialog */}
      {showDeleteConfirm && (
        <div className="fixed inset-0 bg-black/50 dark:bg-black/60 flex items-center justify-center z-50 px-4">
          <div className="bg-white dark:bg-slate-900 rounded-lg shadow-lg p-6 max-w-sm">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
              Delete Recipe
            </h3>
            <p className="text-gray-600 dark:text-gray-400 mb-6">
              Are you sure you want to delete "{recipe.name}"? This action cannot be undone.
            </p>
            <div className="flex gap-3 justify-end">
              <button
                onClick={() => setShowDeleteConfirm(false)}
                className="px-4 py-2 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-slate-800 rounded-lg transition"
              >
                Cancel
              </button>
              <button
                onClick={handleDelete}
                disabled={isDeleting}
                className="px-4 py-2 bg-red-600 hover:bg-red-700 disabled:bg-red-400 text-white font-medium rounded-lg transition"
              >
                {isDeleting ? 'Deleting...' : 'Delete'}
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Main content */}
        <div className="lg:col-span-2">
          {/* Recipe image */}
          <div className="mb-8 rounded-lg overflow-hidden bg-gradient-to-br from-indigo-100 to-indigo-50 dark:from-indigo-900/30 dark:to-indigo-900/10">
            {recipe.imageUrl ? (
              <img
                src={recipe.imageUrl}
                alt={recipe.name}
                className="w-full h-96 object-cover"
              />
            ) : (
              <div className="w-full h-96 flex items-center justify-center">
                <span className="text-6xl">🍳</span>
              </div>
            )}
          </div>

          {/* Header section with title and controls */}
          <div className="mb-8">
            <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-4">
              {recipe.name}
            </h1>

            {/* Description */}
            {recipe.description && (
              <p className="text-lg text-gray-600 dark:text-gray-400 mb-6">
                {recipe.description}
              </p>
            )}

            {/* Recipe meta info */}
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-6 p-4 bg-gray-50 dark:bg-slate-800/50 rounded-lg border border-gray-200 dark:border-slate-700">
              {recipe.cuisineType && (
                <div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">Cuisine</p>
                  <p className="text-lg font-semibold text-gray-900 dark:text-white">
                    {recipe.cuisineType}
                  </p>
                </div>
              )}

              {recipe.prepTime && (
                <div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">Prep Time</p>
                  <p className="text-lg font-semibold text-gray-900 dark:text-white">
                    {recipe.prepTime} min
                  </p>
                </div>
              )}

              {recipe.cookTime && (
                <div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">Cook Time</p>
                  <p className="text-lg font-semibold text-gray-900 dark:text-white">
                    {recipe.cookTime} min
                  </p>
                </div>
              )}

              {recipe.servings && (
                <div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">Servings</p>
                  <p className="text-lg font-semibold text-gray-900 dark:text-white">
                    {recipe.servings}
                  </p>
                </div>
              )}
            </div>
          </div>

          {/* Ingredients */}
          <div className="mb-8">
            <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
              Ingredients
            </h2>
            {(() => {
              const ingredients = Array.isArray(recipe.ingredients)
                ? recipe.ingredients
                : recipe.ingredients?.split(',').map(i => i.trim()) ?? [];
              return (
            <ul className="space-y-2">
              {ingredients && ingredients.length > 0 ? (
                ingredients.map((ingredient, idx) => (
                  <li
                    key={idx}
                    className="flex items-start gap-3 p-3 bg-gray-50 dark:bg-slate-800/30 rounded-lg border border-gray-200 dark:border-slate-700"
                  >
                    <span className="text-indigo-600 dark:text-indigo-400 font-semibold">•</span>
                    <span className="text-gray-700 dark:text-gray-300">{ingredient}</span>
                  </li>
                ))
              ) : (
                <p className="text-gray-600 dark:text-gray-400">No ingredients listed.</p>
              )}
            </ul>
              );
            })()}
          </div>

          {/* Instructions */}
          <div className="mb-8">
            <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
              Instructions
            </h2>
            {(() => {
              const instructions = Array.isArray(recipe.instructions)
                ? recipe.instructions
                : recipe.instructions?.split('\n').map(i => i.trim()).filter(Boolean) ?? [];
              return (
            <ol className="space-y-3">
              {instructions && instructions.length > 0 ? (
                instructions.map((instruction, idx) => (
                  <li
                    key={idx}
                    className="flex gap-4 p-4 bg-gray-50 dark:bg-slate-800/30 rounded-lg border border-gray-200 dark:border-slate-700"
                  >
                    <span className="flex-shrink-0 w-8 h-8 rounded-full bg-indigo-600 dark:bg-indigo-500 text-white flex items-center justify-center font-semibold text-sm">
                      {idx + 1}
                    </span>
                    <span className="text-gray-700 dark:text-gray-300 pt-1">
                      {instruction}
                    </span>
                  </li>
                ))
              ) : (
                <p className="text-gray-600 dark:text-gray-400">No instructions provided.</p>
              )}
            </ol>
              );
            })()}
          </div>
        </div>

        {/* Sidebar */}
        <div className="lg:col-span-1">
          {/* Status and controls */}
          <div className="space-y-4 mb-8">
            {/* Status dropdown */}
            {isOwnRecipe && (
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Status
                </label>
                <select
                  value={recipe.status || 'none'}
                  onChange={(e) => handleStatusChange(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
                >
                  <option value="none">No Status</option>
                  <option value="favourite">Favourite</option>
                  <option value="to-try">To Try</option>
                  <option value="made-before">Made Before</option>
                </select>
              </div>
            )}

            {/* Visibility toggle */}
            {isOwnRecipe && (
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Visibility
                </label>
                <button
                  onClick={handleVisibilityToggle}
                  className={`w-full px-4 py-2 rounded-lg font-medium transition ${
                    recipe.isPublic
                      ? 'bg-green-100 dark:bg-green-900/20 text-green-800 dark:text-green-300 hover:bg-green-200 dark:hover:bg-green-900/30'
                      : 'bg-gray-100 dark:bg-slate-800 text-gray-800 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-slate-700'
                  }`}
                >
                  {recipe.isPublic ? '🔓 Public' : '🔒 Private'}
                </button>
              </div>
            )}

            {/* Edit button */}
            {isOwnRecipe && (
              <button
                onClick={() => navigate(`/recipes/${id}/edit`)}
                className="w-full px-4 py-2 bg-indigo-600 hover:bg-indigo-700 dark:bg-indigo-500 dark:hover:bg-indigo-600 text-white font-medium rounded-lg transition"
              >
                ✏️ Edit
              </button>
            )}

            {/* Delete button */}
            {isOwnRecipe && (
              <button
                onClick={() => setShowDeleteConfirm(true)}
                className="w-full px-4 py-2 bg-red-600 hover:bg-red-700 dark:bg-red-500 dark:hover:bg-red-600 text-white font-medium rounded-lg transition"
              >
                🗑️ Delete
              </button>
            )}
          </div>

          {/* AI Panel - only show for logged in users */}
          {isLoggedIn && (
          <div className="bg-gradient-to-br from-indigo-50 to-indigo-25 dark:from-indigo-900/20 dark:to-slate-800/50 rounded-lg border border-indigo-200 dark:border-indigo-800/50 overflow-hidden">
            <div className="bg-indigo-100 dark:bg-indigo-900/30 px-4 py-3 border-b border-indigo-200 dark:border-indigo-800/50">
              <h3 className="text-lg font-semibold text-indigo-900 dark:text-indigo-100">
                ✨ AI Assistant
              </h3>
            </div>

            <div className="space-y-0">
              {/* Generate New Recipe Section */}
              <div className="border-b border-indigo-200 dark:border-indigo-800/50">
                <button
                  onClick={() =>
                    setExpandedAISection(
                      expandedAISection === 'generate' ? null : 'generate'
                    )
                  }
                  className="w-full px-4 py-3 text-left hover:bg-indigo-100/50 dark:hover:bg-indigo-900/20 transition flex items-center justify-between"
                >
                  <span className="font-medium text-gray-900 dark:text-white">
                    Generate New Recipe
                  </span>
                  <span
                    className={`transform transition ${
                      expandedAISection === 'generate' ? 'rotate-180' : ''
                    }`}
                  >
                    ▼
                  </span>
                </button>

                {expandedAISection === 'generate' && (
                  <div className="px-4 py-4 bg-white/50 dark:bg-slate-800/30">
                    <div className="space-y-3">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          Recipe Name
                        </label>
                        <input
                          type="text"
                          value={generateName}
                          onChange={(e) => setGenerateName(e.target.value)}
                          placeholder="e.g., Chocolate Cake"
                          className="w-full px-3 py-2 border border-indigo-200 dark:border-indigo-800/50 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition text-sm"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          Constraints (optional)
                        </label>
                        <textarea
                          value={generateConstraints}
                          onChange={(e) => setGenerateConstraints(e.target.value)}
                          placeholder="e.g., Vegan, nut-free, under 30 minutes"
                          rows="3"
                          className="w-full px-3 py-2 border border-indigo-200 dark:border-indigo-800/50 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition text-sm"
                        />
                      </div>

                      {aiError.generate && (
                        <p className="text-sm text-red-600 dark:text-red-400">
                          {aiError.generate}
                        </p>
                      )}

                      <button
                        onClick={handleGenerateRecipe}
                        disabled={aiLoading.generate}
                        className="w-full px-3 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 text-white font-medium rounded-lg transition text-sm"
                      >
                        {aiLoading.generate ? 'Generating...' : 'Generate Recipe'}
                      </button>

                      {generatedRecipe && (
                        <div className="mt-4 p-3 bg-indigo-50 dark:bg-indigo-900/20 rounded-lg border border-indigo-200 dark:border-indigo-800/50">
                          <h4 className="font-semibold text-gray-900 dark:text-white mb-2">
                            {generatedRecipe.name}
                          </h4>
                          {generatedRecipe.description && (
                            <p className="text-sm text-gray-700 dark:text-gray-300 mb-3">
                              {generatedRecipe.description}
                            </p>
                          )}
                          <button
                            onClick={handleSaveGeneratedRecipe}
                            className="w-full px-3 py-2 bg-green-600 hover:bg-green-700 text-white font-medium rounded-lg transition text-sm"
                          >
                            Save as New Recipe
                          </button>
                        </div>
                      )}
                    </div>
                  </div>
                )}
              </div>

              {/* Substitute Ingredient Section */}
              <div className="border-b border-indigo-200 dark:border-indigo-800/50">
                <button
                  onClick={() =>
                    setExpandedAISection(
                      expandedAISection === 'substitute' ? null : 'substitute'
                    )
                  }
                  className="w-full px-4 py-3 text-left hover:bg-indigo-100/50 dark:hover:bg-indigo-900/20 transition flex items-center justify-between"
                >
                  <span className="font-medium text-gray-900 dark:text-white">
                    Substitute Ingredient
                  </span>
                  <span
                    className={`transform transition ${
                      expandedAISection === 'substitute' ? 'rotate-180' : ''
                    }`}
                  >
                    ▼
                  </span>
                </button>

                {expandedAISection === 'substitute' && (
                  <div className="px-4 py-4 bg-white/50 dark:bg-slate-800/30">
                    <div className="space-y-3">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          Ingredient to Replace
                        </label>
                        <input
                          type="text"
                          value={substituteIngredient}
                          onChange={(e) => setSubstituteIngredient(e.target.value)}
                          placeholder="e.g., Butter"
                          className="w-full px-3 py-2 border border-indigo-200 dark:border-indigo-800/50 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition text-sm"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          Reason (optional)
                        </label>
                        <input
                          type="text"
                          value={substituteReason}
                          onChange={(e) => setSubstituteReason(e.target.value)}
                          placeholder="e.g., Dairy-free"
                          className="w-full px-3 py-2 border border-indigo-200 dark:border-indigo-800/50 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition text-sm"
                        />
                      </div>

                      {aiError.substitute && (
                        <p className="text-sm text-red-600 dark:text-red-400">
                          {aiError.substitute}
                        </p>
                      )}

                      <button
                        onClick={handleSubstituteIngredient}
                        disabled={aiLoading.substitute}
                        className="w-full px-3 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 text-white font-medium rounded-lg transition text-sm"
                      >
                        {aiLoading.substitute ? 'Finding...' : 'Find Substitutes'}
                      </button>

                      {substitutionResults && (
                        <div className="mt-4 p-3 bg-indigo-50 dark:bg-indigo-900/20 rounded-lg border border-indigo-200 dark:border-indigo-800/50">
                          <h4 className="font-semibold text-gray-900 dark:text-white mb-2">
                            Suggested Substitutes
                          </h4>
                          {Array.isArray(substitutionResults) ? (
                            <ul className="text-sm text-gray-700 dark:text-gray-300 space-y-1">
                              {substitutionResults.map((item, idx) => (
                                <li key={idx}>• {item}</li>
                              ))}
                            </ul>
                          ) : (
                            <p className="text-sm text-gray-700 dark:text-gray-300">
                              {substitutionResults}
                            </p>
                          )}
                        </div>
                      )}
                    </div>
                  </div>
                )}
              </div>

              {/* Tweak Recipe Section */}
              <div>
                <button
                  onClick={() =>
                    setExpandedAISection(expandedAISection === 'tweak' ? null : 'tweak')
                  }
                  className="w-full px-4 py-3 text-left hover:bg-indigo-100/50 dark:hover:bg-indigo-900/20 transition flex items-center justify-between"
                >
                  <span className="font-medium text-gray-900 dark:text-white">
                    Tweak Recipe
                  </span>
                  <span
                    className={`transform transition ${
                      expandedAISection === 'tweak' ? 'rotate-180' : ''
                    }`}
                  >
                    ▼
                  </span>
                </button>

                {expandedAISection === 'tweak' && (
                  <div className="px-4 py-4 bg-white/50 dark:bg-slate-800/30">
                    <div className="space-y-3">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          What would you like to change?
                        </label>
                        <textarea
                          value={tweakRequest}
                          onChange={(e) => setTweakRequest(e.target.value)}
                          placeholder="e.g., Make it spicier, reduce sugar, make it vegan"
                          rows="3"
                          className="w-full px-3 py-2 border border-indigo-200 dark:border-indigo-800/50 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition text-sm"
                        />
                      </div>

                      {aiError.tweak && (
                        <p className="text-sm text-red-600 dark:text-red-400">
                          {aiError.tweak}
                        </p>
                      )}

                      <button
                        onClick={handleTweakRecipe}
                        disabled={aiLoading.tweak}
                        className="w-full px-3 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 text-white font-medium rounded-lg transition text-sm"
                      >
                        {aiLoading.tweak ? 'Tweaking...' : 'Suggest Changes'}
                      </button>

                      {tweakedRecipe && (
                        <div className="mt-4 p-3 bg-indigo-50 dark:bg-indigo-900/20 rounded-lg border border-indigo-200 dark:border-indigo-800/50">
                          <h4 className="font-semibold text-gray-900 dark:text-white mb-2">
                            Suggested Changes
                          </h4>
                          {tweakedRecipe.description && (
                            <p className="text-sm text-gray-700 dark:text-gray-300 mb-3">
                              {tweakedRecipe.description}
                            </p>
                          )}
                          {tweakedRecipe.ingredients && (
                            <div className="mb-3">
                              <p className="text-xs font-semibold text-gray-700 dark:text-gray-300 mb-1">
                                Updated Ingredients:
                              </p>
                              <ul className="text-xs text-gray-700 dark:text-gray-300 space-y-1">
                                {tweakedRecipe.ingredients.map((ing, idx) => (
                                  <li key={idx}>• {ing}</li>
                                ))}
                              </ul>
                            </div>
                          )}
                          <button
                            onClick={handleApplyTweaks}
                            className="w-full px-3 py-2 bg-green-600 hover:bg-green-700 text-white font-medium rounded-lg transition text-sm"
                          >
                            Apply Changes
                          </button>
                        </div>
                      )}
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
          )}
        </div>
      </div>
    </div>
  );
}
