import { useState, useCallback, useRef } from 'react';
import { SearchResponse } from '../types';

export const useSearch = () => {
  const [searchResults, setSearchResults] = useState<SearchResponse | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const search = useCallback(async (query: string) => {
    if (!query.trim()) return;
    setIsLoading(true);
    setError(null);
    try {
      const response = await fetch(
      `${import.meta.env.VITE_API_BASE_URL}/api/Question/similar?title=${encodeURIComponent(query)}`
      );
      if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
      }
      const data = await response.json();
      setSearchResults(data);
    } catch (err) {
      setError('Failed to search. Please try again.');
      console.error('Search error:', err);
    } finally {
      setIsLoading(false);
    }
  }, []);

    const aiSort = useCallback(async (query: string) => {
    if (!query.trim()) return;
    setIsLoading(true);
    setError(null);
    try {
      const response = await fetch(
      `${import.meta.env.VITE_API_BASE_URL}/api/Question/ranked?title=${encodeURIComponent(query)}`
      );
      if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
      }
      const data = await response.json();
      setSearchResults(data);
    } catch (err) {
      setError('Failed to search. Please try again.');
      console.error('Search error:', err);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const clearResults = useCallback(() => {
    setSearchResults(null);
    setError(null);
  }, []);

  return {
    searchResults,
    isLoading,
    error,
    search,
    aiSort,
    clearResults
  };
};