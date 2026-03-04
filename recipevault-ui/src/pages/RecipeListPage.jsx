import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useContext } from 'react';
import { AuthContext } from '../context/AuthContext';
import api from '../api/axios';

/*
 * RecipeListPage displays recipes with two tabs: "My Recipes" and "Public Recipes".
 *
 * Features:
 * - Two tabs to switch between personal and public recipes
 * - Search functionality across recipes (name, cuisine type, prep time)
 * - Recipe cards with name, cuisine, prep time, and status badge
 * - Color-coded status badges: green (favourite), blue (to-try), yellow (made-before)
 * - Click to view recipe details
 * - "Add Recipe" button for creating new recipes
 * - Loading and empty states
 * - Full dark mode support
 * - Indigo accent color for UI elements
 */

export function RecipeListPage() {
  const navigate = useNavigate();
  const { token } = useContext(AuthContext);

  const [activeTab, setActiveTab] = useState('my-recipes'); // 'my-recipes' or 'public'
  const [recipes, setRecipes] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  // Search filters
  const [searchQuery, setSearchQuery] = useState('');
  const [cuisineFilter, setCuisineFilter] = useState('');
  const [maxPrepTime, setMaxPrepTime] = useState('');

  // Debounce search
  const [searchTimeout, setSearchTimeout] = useState(null);

  // Fetch recipes based on active tab and search filters
  const fetchRecipes = useCallback(async () => {
    setIsLoading(true);
    setError('');

    try {
      let endpoint;
      let params = {};

      if (activeTab === 'my-recipes') {
        endpoint = '/recipes';
      } else {
        endpoint = '/recipes/public';
      }

      // Add search filters if provided
      if (searchQuery) params.name = searchQuery;
      if (cuisineFilter) params.cuisineType = cuisineFilter;
      if (maxPrepTime) params.maxPrepTime = parseInt(maxPrepTime, 10);

      const response = await api.get(endpoint, { params });
      setRecipes(response.data || []);
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to fetch recipes. Please try again.';
      setError(errorMessage);
      setRecipes([]);
    } finally {
      setIsLoading(false);
    }
  }, [activeTab, searchQuery, cuisineFilter, maxPrepTime]);

  // Fetch recipes when tab changes or filters change
  useEffect(() => {
    fetchRecipes();
  }, [fetchRecipes]);

  // Debounced search handler
  const handleSearchChange = (e) => {
    const value = e.target.value;
    setSearchQuery(value);

    // Clear existing timeout
    if (searchTimeout) clearTimeout(searchTimeout);

    // Set new timeout for debounced search
    const timeout = setTimeout(() => {
      // Search will be triggered by the effect dependency
    }, 300);

    setSearchTimeout(timeout);
  };

  // Navigate to recipe detail page
  const goToRecipeDetail = (recipeId) => {
    navigate(`/recipes/${recipeId}`);
  };

  // Get badge color based on status
  const getStatusBadgeColor = (status) => {
    switch (status?.toLowerCase()) {
      case 'favourite':
      case 'favorite':
        return 'bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-300';
      case 'to-try':
      case 'totry':
        return 'bg-blue-100 dark:bg-blue-900/30 text-blue-800 dark:text-blue-300';
      case 'made-before':
      case 'madebefore':
        return 'bg-yellow-100 dark:bg-yellow-900/30 text-yellow-800 dark:text-yellow-300';
      default:
        return 'bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-300';
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Header with title and Add Recipe button */}
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
          Recipes
        </h1>
        <button
          onClick={() => navigate('/recipes/new')}
          className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 dark:bg-indigo-500 dark:hover:bg-indigo-600 text-white font-medium rounded-lg transition"
        >
          + Add Recipe
        </button>
      </div>

      {/* Tab navigation */}
      <div className="flex gap-4 mb-6 border-b border-gray-200 dark:border-slate-700">
        <button
          onClick={() => setActiveTab('my-recipes')}
          className={`px-4 py-2 font-medium transition border-b-2 ${
            activeTab === 'my-recipes'
              ? 'border-indigo-600 dark:border-indigo-500 text-indigo-600 dark:text-indigo-400'
              : 'border-transparent text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-300'
          }`}
        >
          My Recipes
        </button>
        <button
          onClick={() => setActiveTab('public')}
          className={`px-4 py-2 font-medium transition border-b-2 ${
            activeTab === 'public'
              ? 'border-indigo-600 dark:border-indigo-500 text-indigo-600 dark:text-indigo-400'
              : 'border-transparent text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-300'
          }`}
        >
          Public Recipes
        </button>
      </div>

      {/* Search and filter section */}
      <div className="mb-8 space-y-4">
        {/* Search bar */}
        <div className="relative">
          <input
            type="text"
            placeholder="Search recipes by name..."
            value={searchQuery}
            onChange={handleSearchChange}
            className="w-full px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
          />
          {searchQuery && (
            <button
              onClick={() => setSearchQuery('')}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
            >
              ✕
            </button>
          )}
        </div>

        {/* Filter controls */}
        <div className="flex gap-4 flex-col sm:flex-row">
          <input
            type="text"
            placeholder="Cuisine type (e.g., Italian, Asian)"
            value={cuisineFilter}
            onChange={(e) => setCuisineFilter(e.target.value)}
            className="flex-1 px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
          />
          <input
            type="number"
            placeholder="Max prep time (minutes)"
            value={maxPrepTime}
            onChange={(e) => setMaxPrepTime(e.target.value)}
            min="0"
            className="flex-1 px-4 py-2 border border-gray-300 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:focus:ring-indigo-400 transition"
          />
          {(searchQuery || cuisineFilter || maxPrepTime) && (
            <button
              onClick={() => {
                setSearchQuery('');
                setCuisineFilter('');
                setMaxPrepTime('');
              }}
              className="px-4 py-2 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-300 border border-gray-300 dark:border-slate-700 rounded-lg transition"
            >
              Clear Filters
            </button>
          )}
        </div>
      </div>

      {/* Error message */}
      {error && (
        <div className="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
          <p className="text-sm text-red-800 dark:text-red-300">{error}</p>
        </div>
      )}

      {/* Loading state */}
      {isLoading && (
        <div className="flex items-center justify-center py-16">
          <div className="flex flex-col items-center gap-4">
            <div className="w-8 h-8 border-4 border-gray-300 dark:border-slate-700 border-t-indigo-600 dark:border-t-indigo-500 rounded-full animate-spin"></div>
            <p className="text-gray-600 dark:text-gray-400">Loading recipes...</p>
          </div>
        </div>
      )}

      {/* Empty state */}
      {!isLoading && recipes.length === 0 && (
        <div className="flex flex-col items-center justify-center py-16 text-center">
          <div className="text-5xl mb-4">🍽️</div>
          <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
            {searchQuery || cuisineFilter || maxPrepTime ? 'No recipes found' : 'No recipes yet'}
          </h3>
          <p className="text-gray-600 dark:text-gray-400 mb-6">
            {searchQuery || cuisineFilter || maxPrepTime
              ? 'Try adjusting your search filters'
              : activeTab === 'my-recipes'
                ? 'Create your first recipe to get started!'
                : 'No public recipes available yet'}
          </p>
          {activeTab === 'my-recipes' && !searchQuery && !cuisineFilter && !maxPrepTime && (
            <button
              onClick={() => navigate('/recipes/new')}
              className="px-6 py-2 bg-indigo-600 hover:bg-indigo-700 dark:bg-indigo-500 dark:hover:bg-indigo-600 text-white font-medium rounded-lg transition"
            >
              Create Your First Recipe
            </button>
          )}
        </div>
      )}

      {/* Recipe grid */}
      {!isLoading && recipes.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {recipes.map((recipe) => (
            <div
              key={recipe.id}
              onClick={() => goToRecipeDetail(recipe.id)}
              className="bg-white dark:bg-slate-900 rounded-lg shadow-md hover:shadow-lg dark:hover:shadow-lg dark:shadow-black/50 dark:hover:shadow-black/70 transition cursor-pointer overflow-hidden border border-gray-200 dark:border-slate-800 hover:border-indigo-500 dark:hover:border-indigo-400"
            >
              {/* Recipe image placeholder */}
              {recipe.imageUrl && (
                <div className="w-full h-48 bg-gray-200 dark:bg-slate-800 overflow-hidden">
                  <img
                    src={recipe.imageUrl}
                    alt={recipe.name}
                    className="w-full h-full object-cover"
                  />
                </div>
              )}
              {!recipe.imageUrl && (
                <div className="w-full h-48 bg-gradient-to-br from-indigo-100 to-indigo-50 dark:from-indigo-900/30 dark:to-indigo-900/10 flex items-center justify-center">
                  <span className="text-4xl">🍳</span>
                </div>
              )}

              {/* Recipe content */}
              <div className="p-4">
                {/* Title */}
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2 line-clamp-2">
                  {recipe.name}
                </h3>

                {/* Cuisine type */}
                {recipe.cuisineType && (
                  <p className="text-sm text-gray-600 dark:text-gray-400 mb-3">
                    {recipe.cuisineType}
                  </p>
                )}

                {/* Prep time and status badge row */}
                <div className="flex items-center justify-between mb-3">
                  {/* Prep time */}
                  {recipe.prepTime && (
                    <div className="flex items-center gap-2">
                      <span className="text-sm text-gray-600 dark:text-gray-400">⏱️</span>
                      <span className="text-sm text-gray-600 dark:text-gray-400">
                        {recipe.prepTime} min
                      </span>
                    </div>
                  )}

                  {/* Status badge */}
                  {recipe.status && (
                    <span
                      className={`text-xs font-medium px-2 py-1 rounded-full ${getStatusBadgeColor(
                        recipe.status
                      )}`}
                    >
                      {recipe.status}
                    </span>
                  )}
                </div>

                {/* Description preview */}
                {recipe.description && (
                  <p className="text-sm text-gray-600 dark:text-gray-400 line-clamp-2">
                    {recipe.description}
                  </p>
                )}
              </div>

              {/* Card footer with action hint */}
              <div className="px-4 py-3 bg-gray-50 dark:bg-slate-800/50 border-t border-gray-200 dark:border-slate-700">
                <p className="text-xs text-indigo-600 dark:text-indigo-400 font-medium">
                  Click to view details →
                </p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
