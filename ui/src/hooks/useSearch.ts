import { useState, useCallback } from 'react';
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
      // Simulate API call with sample data
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      const sampleResults: SearchResponse = {
        items: [
          {
            question_id: 1,
            title: `How to implement ${query} in React with TypeScript?`,
            link: 'https://stackoverflow.com/questions/sample-1',
            score: 42,
            answer_count: 8,
            view_count: 15420,
            tags: ['react', 'typescript', query.toLowerCase()],
            is_answered: true,
            creation_date: Date.now() / 1000 - 86400 * 2, // 2 days ago
            owner: {
              display_name: 'John Developer',
              reputation: 12500
            }
          },
          {
            question_id: 2,
            title: `Best practices for ${query} optimization`,
            link: 'https://stackoverflow.com/questions/sample-2',
            score: 28,
            answer_count: 3,
            view_count: 8230,
            tags: ['performance', 'optimization', query.toLowerCase()],
            is_answered: true,
            creation_date: Date.now() / 1000 - 86400 * 5, // 5 days ago
            owner: {
              display_name: 'Sarah Coder',
              reputation: 8900
            }
          },
          {
            question_id: 3,
            title: `${query} error: Cannot resolve module`,
            link: 'https://stackoverflow.com/questions/sample-3',
            score: 15,
            answer_count: 2,
            view_count: 3450,
            tags: ['error', 'debugging', query.toLowerCase()],
            is_answered: false,
            creation_date: Date.now() / 1000 - 86400, // 1 day ago
            owner: {
              display_name: 'Mike Newbie',
              reputation: 250
            }
          },
          {
            question_id: 4,
            title: `Advanced ${query} patterns and techniques`,
            link: 'https://stackoverflow.com/questions/sample-4',
            score: 67,
            answer_count: 12,
            view_count: 23100,
            tags: ['advanced', 'patterns', query.toLowerCase()],
            is_answered: true,
            creation_date: Date.now() / 1000 - 86400 * 10, // 10 days ago
            owner: {
              display_name: 'Expert Dev',
              reputation: 45000
            }
          },
          {
            question_id: 5,
            title: `How to test ${query} components effectively?`,
            link: 'https://stackoverflow.com/questions/sample-5',
            score: 33,
            answer_count: 6,
            view_count: 7800,
            tags: ['testing', 'unit-testing', query.toLowerCase()],
            is_answered: true,
            creation_date: Date.now() / 1000 - 86400 * 3, // 3 days ago
            owner: {
              display_name: 'QA Master',
              reputation: 18750
            }
          }
        ],
        has_more: true,
        quota_max: 300,
        quota_remaining: 295,
        total: 1234
      };

      setSearchResults(sampleResults);
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
    clearResults
  };
};