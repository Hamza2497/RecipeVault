import { useState, useEffect, useCallback, useContext } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';
import api from '../api/axios';

/*
 * AddEditRecipePage is a form for creating or editing a recipe.
 *
 * Features:
 * - If URL is /recipes/new: create mode with empty form, submit via POST
 * - If URL is /recipes/:id/edit: edit mode, fetch recipe, pre-fill form, submit via PUT
 * - Form fields: name, description, ingredients, instructions, cuisine type, prep/cook time, servings, status, isPublic
 * - Image generation: "Generate Image" button next to name field, "Regenerate" button below preview
 * - AI recipe generation: collapsible panel to auto-fill all fields with AI
 * - Full dark mode support with indigo accent color
 * - Cancel button returns to previous page
 * - Save button shows loading state during submission
 */

export function AddEditRecipePage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useContext(AuthContext);

  // Determine if we're creating or editing
  const isEditing = !!id;

  // Recipe fetch state
  const [isLoadingRecipe, setIsLoadingRecipe] = useState(isEditing);
  const [recipeError, setRecipeError] = useState('');

  // Form fields
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [ingredients, setIngredients] = useState('');
  const [instructions, setInstructions] = useState('');
  const [cuisineType, setCuisineType] = useState('');
  const [prepTimeMinutes, setPrepTimeMinutes] = useState('');
  const [cookTimeMinutes, setCookTimeMinutes] = useState('');
  const [servings, setServings] = useState('');
  const [status, setStatus] = useState('none');
  const [isPublic, setIsPublic] = useState(false);

  // Image generation state
  const [imageUrl, setImageUrl] = useState('');
  const [imageLoading, setImageLoading] = useState(false);
  const [imageError, setImageError] = useState('');

  // AI recipe generation state
  const [aiExpanded, setAiExpanded] = useState(false);
  const [aiLoading, setAiLoading] = useState(false);
  const [aiError, setAiError] = useState('');
  const [aiMaxPrepTime, setAiMaxPrepTime] = useState('');
  const [aiMaxCookTime, setAiMaxCookTime] = useState('');
  const [aiAllergies, setAiAllergies] = useState('');
  const [aiDietaryRestrictions, setAiDietaryRestrictions] = useState('');

  // Form submission state
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState('');

  // Fetch recipe if editing
  const fetchRecipe = useCallback(async () => {
    setIsLoadingRecipe(true);
    setRecipeError('');

    try {
      const response = await api.get(`/recipes/${id}`);
      const recipe = response.data;
      setName(recipe.name || '');
      setDescription(recipe.description || '');
      setIngredients(
        Array.isArray(recipe.ingredients)
          ? recipe.ingredients.join(', ')
          : recipe.ingredients || ''
      );
      setInstructions(
        Array.isArray(recipe.instructions)
          ? recipe.instructions.join('\n')
          : recipe.instructions || ''
      );
      setCuisineType(recipe.cuisineType || '');
      setPrepTimeMinutes(recipe.prepTimeMinutes || '');
      setCookTimeMinutes(recipe.cookTimeMinutes || '');
      setServings(recipe.servings || '');
      setStatus(recipe.status || 'none');
      setIsPublic(recipe.isPublic || false);
      setImageUrl(recipe.imageUrl || '');
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to load recipe. Please try again.';
      setRecipeError(errorMessage);
    } finally {
      setIsLoadingRecipe(false);
    }
  }, [id]);

  useEffect(() => {
    if (isEditing) {
      fetchRecipe();
    }
  }, [isEditing, fetchRecipe]);

  // Generate image
  const handleGenerateImage = async () => {
    if (!name.trim()) {
      setImageError('Please enter a recipe name first.');
      return;
    }

    setImageLoading(true);
    setImageError('');

    try {
      const response = await api.post('/ai/generate-image', {
        recipeName: name,
        cuisineType: cuisineType || 'general',
      });
      setImageUrl(response.data.imageUrl);
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to generate image. Please try again.';
      setImageError(errorMessage);
    } finally {
      setImageLoading(false);
    }
  };

  // Generate recipe with AI
  const handleGenerateWithAI = async () => {
    if (!name.trim()) {
      setAiError('Please enter a recipe name in the main form.');
      return;
    }

    setAiLoading(true);
    setAiError('');

    try {
      const response = await api.post('/ai/generate', {
        recipeName: name,
        cuisineType: cuisineType || '',
        maxPrepTimeMinutes: aiMaxPrepTime ? parseInt(aiMaxPrepTime) : 0,
        maxCookTimeMinutes: aiMaxCookTime ? parseInt(aiMaxCookTime) : 0,
        allergies: aiAllergies
          .split(',')
          .map(a => a.trim())
          .filter(Boolean),
        dietaryRestrictions: aiDietaryRestrictions
          .split(',')
          .map(d => d.trim())
          .filter(Boolean),
      });

      const generatedRecipe = response.data;
      setName(generatedRecipe.name || '');
      setDescription(generatedRecipe.description || '');
      setIngredients(
        Array.isArray(generatedRecipe.ingredients)
          ? generatedRecipe.ingredients.join(', ')
          : generatedRecipe.ingredients || ''
      );
      setInstructions(
        Array.isArray(generatedRecipe.instructions)
          ? generatedRecipe.instructions.join('\n')
          : generatedRecipe.instructions || ''
      );
      setCuisineType(generatedRecipe.cuisineType || '');
      setPrepTimeMinutes(generatedRecipe.prepTimeMinutes || '');
      setCookTimeMinutes(generatedRecipe.cookTimeMinutes || '');
      setServings(generatedRecipe.servings || '');

      setAiExpanded(false);
      setAiMaxPrepTime('');
      setAiMaxCookTime('');
      setAiAllergies('');
      setAiDietaryRestrictions('');
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to generate recipe. Please try again.';
      setAiError(errorMessage);
    } finally {
      setAiLoading(false);
    }
  };

  // Submit form
  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!name.trim()) {
      setSubmitError('Please enter a recipe name.');
      return;
    }

    setIsSubmitting(true);
    setSubmitError('');

    try {
      const payload = {
        name: name.trim(),
        description: description.trim(),
        ingredients: ingredients.trim(),
        instructions: instructions.trim(),
        cuisineType: cuisineType.trim(),
        prepTimeMinutes: parseInt(prepTimeMinutes) || 0,
        cookTimeMinutes: parseInt(cookTimeMinutes) || 0,
        servings: parseInt(servings) || 0,
        imageUrl: imageUrl || null,
        status,
        isPublic,
      };

      let response;
      if (isEditing) {
        response = await api.put(`/recipes/${id}`, payload);
      } else {
        response = await api.post('/recipes', payload);
      }

      const recipeId = response.data.id || id;
      navigate(`/recipes/${recipeId}`);
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to save recipe. Please try again.';
      setSubmitError(errorMessage);
    } finally {
      setIsSubmitting(false);
    }
  };

  // Loading state for fetching existing recipe
  if (isLoadingRecipe) {
    return (
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="flex items-center justify-center py-16">
          <div className="flex flex-col items-center gap-4">
            <div className="w-8 h-8 border-4 border-gray-300 dark:border-slate-700 border-t-indigo-600 dark:border-t-indigo-500 rounded-full animate-spin"></div>
            <p className="text-gray-600 dark:text-gray-400">Loading recipe...</p>
          </div>
        </div>
      </div>
    );
  }

  // Error state for fetching recipe
  if (recipeError && isEditing) {
    return (
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
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

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Back button */}
      <button
        onClick={() => navigate(-1)}
        className="mb-6 px-4 py-2 text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 dark:hover:text-indigo-300 font-medium transition"
      >
        ← Back
      </button>

      {/* Page title */}
      <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-8">
        {isEditing ? 'Edit Recipe' : 'Add New Recipe'}
      </h1>

      {/* Submit error */}
      {submitError && (
        <div className="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
          <p className="text-sm text-red-800 dark:text-red-300">{submitError}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-8">
        {/* Recipe Name and Image Generation */}
        <div className="space-y-4">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex-1">
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Recipe Name *
              </label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g., Chocolate Cake"
                className="w-full px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
              />
            </div>

            <div className="flex items-end">
              <button
                type="button"
                onClick={handleGenerateImage}
                disabled={imageLoading}
                className="px-6 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 text-white font-medium rounded-lg transition whitespace-nowrap"
              >
                {imageLoading ? 'Generating...' : '🎨 Generate Image'}
              </button>
            </div>
          </div>

          {/* Image error */}
          {imageError && (
            <p className="text-sm text-red-600 dark:text-red-400">{imageError}</p>
          )}

          {/* Image preview */}
          {imageUrl && (
            <div className="space-y-3">
              <div className="rounded-lg overflow-hidden bg-gradient-to-br from-indigo-100 to-indigo-50 dark:from-indigo-900/30 dark:to-indigo-900/10">
                <img
                  src={imageUrl}
                  alt={name || 'Recipe'}
                  className="w-full h-64 object-cover"
                />
              </div>
              <button
                type="button"
                onClick={handleGenerateImage}
                disabled={imageLoading}
                className="px-4 py-2 bg-gray-200 dark:bg-slate-700 hover:bg-gray-300 dark:hover:bg-slate-600 disabled:bg-gray-100 dark:disabled:bg-slate-800 text-gray-900 dark:text-white font-medium rounded-lg transition"
              >
                {imageLoading ? 'Regenerating...' : '🔄 Regenerate Image'}
              </button>
            </div>
          )}
        </div>

        {/* Description */}
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Description
          </label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="A brief description of your recipe..."
            rows="3"
            className="w-full px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
          />
        </div>

        {/* Ingredients */}
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Ingredients (comma separated)
          </label>
          <textarea
            value={ingredients}
            onChange={(e) => setIngredients(e.target.value)}
            placeholder="e.g., 2 cups flour, 1 egg, 1 cup sugar"
            rows="4"
            className="w-full px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
          />
        </div>

        {/* Instructions */}
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Instructions (one step per line)
          </label>
          <textarea
            value={instructions}
            onChange={(e) => setInstructions(e.target.value)}
            placeholder="Step 1&#10;Step 2&#10;Step 3"
            rows="5"
            className="w-full px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
          />
        </div>

        {/* Recipe metadata */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Cuisine Type
            </label>
            <input
              type="text"
              value={cuisineType}
              onChange={(e) => setCuisineType(e.target.value)}
              placeholder="e.g., Italian"
              className="w-full px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Prep Time (min)
            </label>
            <input
              type="number"
              value={prepTimeMinutes}
              onChange={(e) => setPrepTimeMinutes(e.target.value)}
              placeholder="15"
              min="0"
              className="w-full px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Cook Time (min)
            </label>
            <input
              type="number"
              value={cookTimeMinutes}
              onChange={(e) => setCookTimeMinutes(e.target.value)}
              placeholder="30"
              min="0"
              className="w-full px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Servings
            </label>
            <input
              type="number"
              value={servings}
              onChange={(e) => setServings(e.target.value)}
              placeholder="4"
              min="0"
              className="w-full px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
            />
          </div>
        </div>

        {/* Status and visibility */}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Status
            </label>
            <select
              value={status}
              onChange={(e) => setStatus(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
            >
              <option value="none">No Status</option>
              <option value="favourite">Favourite</option>
              <option value="to-try">To Try</option>
              <option value="made-before">Made Before</option>
            </select>
          </div>

          <div className="flex items-end">
            <label className="flex items-center gap-3 p-3 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 cursor-pointer hover:bg-gray-50 dark:hover:bg-slate-700 transition w-full">
              <input
                type="checkbox"
                checked={isPublic}
                onChange={(e) => setIsPublic(e.target.checked)}
                className="w-5 h-5 rounded border-gray-300 dark:border-slate-600 text-indigo-600 dark:text-indigo-500 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
              />
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                Make this recipe public
              </span>
            </label>
          </div>
        </div>

        {/* AI Generate Recipe Panel */}
        <div className="bg-gradient-to-br from-indigo-50 to-indigo-25 dark:from-indigo-900/20 dark:to-slate-800/50 rounded-lg border border-indigo-200 dark:border-indigo-800/50 overflow-hidden">
          <button
            type="button"
            onClick={() => setAiExpanded(!aiExpanded)}
            className="w-full px-6 py-4 text-left hover:bg-indigo-100/50 dark:hover:bg-indigo-900/20 transition flex items-center justify-between bg-indigo-100 dark:bg-indigo-900/30 border-b border-indigo-200 dark:border-indigo-800/50"
          >
            <span className="text-lg font-semibold text-indigo-900 dark:text-indigo-100">
              ✨ Generate with AI
            </span>
            <span
              className={`transform transition ${aiExpanded ? 'rotate-180' : ''}`}
            >
              ▼
            </span>
          </button>

          {aiExpanded && (
            <div className="px-6 py-4 bg-white/50 dark:bg-slate-800/30 space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Max Prep Time (min)
                  </label>
                  <input
                    type="number"
                    value={aiMaxPrepTime}
                    onChange={(e) => setAiMaxPrepTime(e.target.value)}
                    placeholder="30"
                    min="0"
                    className="w-full px-4 py-2 border border-indigo-200 dark:border-indigo-800/50 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Max Cook Time (min)
                  </label>
                  <input
                    type="number"
                    value={aiMaxCookTime}
                    onChange={(e) => setAiMaxCookTime(e.target.value)}
                    placeholder="60"
                    min="0"
                    className="w-full px-4 py-2 border border-indigo-200 dark:border-indigo-800/50 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Allergies (comma separated)
                </label>
                <input
                  type="text"
                  value={aiAllergies}
                  onChange={(e) => setAiAllergies(e.target.value)}
                  placeholder="e.g., nuts, dairy, shellfish"
                  className="w-full px-4 py-2 border border-indigo-200 dark:border-indigo-800/50 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Dietary Restrictions (comma separated)
                </label>
                <input
                  type="text"
                  value={aiDietaryRestrictions}
                  onChange={(e) => setAiDietaryRestrictions(e.target.value)}
                  placeholder="e.g., vegan, gluten-free, keto"
                  className="w-full px-4 py-2 border border-indigo-200 dark:border-indigo-800/50 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
                />
              </div>

              {aiError && (
                <p className="text-sm text-red-600 dark:text-red-400">{aiError}</p>
              )}

              <button
                type="button"
                onClick={handleGenerateWithAI}
                disabled={aiLoading}
                className="w-full px-4 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 text-white font-medium rounded-lg transition"
              >
                {aiLoading ? 'Generating...' : 'Apply'}
              </button>
            </div>
          )}
        </div>

        {/* Form actions */}
        <div className="flex gap-4 pt-4">
          <button
            type="button"
            onClick={() => navigate(-1)}
            className="flex-1 px-6 py-3 border border-gray-300 dark:border-slate-700 text-gray-900 dark:text-white font-medium rounded-lg hover:bg-gray-50 dark:hover:bg-slate-800 transition"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            className="flex-1 px-6 py-3 bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 text-white font-medium rounded-lg transition"
          >
            {isSubmitting ? 'Saving...' : isEditing ? 'Update Recipe' : 'Create Recipe'}
          </button>
        </div>
      </form>
    </div>
  );
}
