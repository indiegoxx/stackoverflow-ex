import React, { useState } from 'react';
import { Sparkles, Clock, TrendingUp } from 'lucide-react';
import { SearchResponse, TabType } from '../types';
import SearchResultBox from './SearchResultBox';
import { AISearchResults } from './AISearchResults';

interface SearchResultsProps {
  results: SearchResponse;
  query: string;
  isLoading?: boolean;
}

const SearchResults: React.FC<SearchResultsProps> = ({ results, query, isLoading = false }) => {
  const [activeTab, setActiveTab] = useState<TabType>('general');

  const tabs = [
    { id: 'general' as TabType, label: 'All Results', icon: null, count: results.items.length },
    { id: 'ai' as TabType, label: 'Sort By AI', icon: Sparkles,},
  ];


  const filteredResults = results.items.filter(item => {
    switch (activeTab) {
      case 'ai':
        return true;
      case 'recent':
        return true; 
      default:
        return true;
    }
  });


  if (isLoading) {
    return (
      <div className="flex-1 p-6 max-w-4xl mx-auto">
        <div className="animate-pulse">
          <div className="h-8 bg-gray-200 rounded w-1/4 mb-6"></div>
          <div className="space-y-4">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="p-6 border border-gray-200 rounded-md">
                <div className="h-6 bg-gray-200 rounded w-3/4 mb-3"></div>
                <div className="h-4 bg-gray-200 rounded w-1/2 mb-4"></div>
                <div className="flex space-x-4">
                  <div className="h-4 bg-gray-200 rounded w-16"></div>
                  <div className="h-4 bg-gray-200 rounded w-16"></div>
                  <div className="h-4 bg-gray-200 rounded w-16"></div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 bg-gray-50 min-h-screen font-['Inter']">
      {/* Header for search results */}
      <div className="bg-white border-b border-gray-200 py-6 px-6 sm:px-8">
        <div className="max-w-4xl mx-auto flex flex-col sm:flex-row sm:items-center sm:justify-between mb-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-900 mb-2">
              Search Results for "{query}"
            </h1>
            <p className="text-gray-600 text-sm">
              Found {results.items.length} questions • {results.quota_remaining} searches remaining
            </p>
          </div>
          <div className="mt-4 sm:mt-0 flex items-center space-x-2 text-sm text-gray-500">
            <TrendingUp className="w-4 h-4" />
            <span>Sorted by relevance</span>
          </div>
        </div>

        {/* Tab Navigation */}
        <div className="max-w-4xl mx-auto flex border border-gray-300 rounded-md overflow-hidden text-sm">
          {tabs.map(({ id, label, icon: Icon, count }) => (
            <button
              key={id}
              className={`flex items-center justify-center flex-1 space-x-2 py-2 px-4 transition-all duration-200 
                ${activeTab === id
                  ? 'bg-blue-500 text-white font-medium' // Active tab style
                  : 'bg-white text-gray-700 hover:bg-gray-100' // Inactive tab style
                }`}
              onClick={() => setActiveTab(id)}
            >
              {Icon && <Icon className="w-4 h-4" />}
              <span>{label}</span>
              {/* Count badge - keep it subtle for Stack Overflow feel */}
                {count && (
                <span className={`text-xs px-2 py-0.5 rounded-full 
                  ${activeTab === id ? 'bg-blue-600 text-white' : 'bg-gray-200 text-gray-700'}`}>
                  {count}
                </span>
                )}
            </button>
          ))}
        </div>
      </div>

      {/* Main content area for results */}
      <div className="p-6 max-w-4xl mx-auto">
        <div className="space-y-4">
          {activeTab === 'ai' ? (
            <>
            <AISearchResults query={query} />
            </>
          ) : (
        filteredResults.map((item) => (
          <SearchResultBox
            key={item.question_id}
            item={item}
          />
        ))
          )}
        </div>
      </div>
    </div>
  );
};

export default SearchResults;
