import React, { useState } from 'react';
import { ArrowUp, MessageCircle, Eye, Tag, ExternalLink, Sparkles, Clock, TrendingUp } from 'lucide-react';
import { SearchResponse, TabType } from '../types';

interface SearchResultsProps {
  results: SearchResponse;
  query: string;
  isLoading?: boolean;
}

const SearchResults: React.FC<SearchResultsProps> = ({ results, query, isLoading = false }) => {
  const [activeTab, setActiveTab] = useState<TabType>('general');

  const tabs = [
    { id: 'general' as TabType, label: 'All Results', icon: null, count: results.items.length },
    { id: 'ai' as TabType, label: 'AI Answered', icon: Sparkles, count: results.items.filter(item => item.is_answered).length },
    { id: 'recent' as TabType, label: 'Recent', icon: Clock, count: results.items.length },
  ];

  const filteredResults = results.items.filter(item => {
    switch (activeTab) {
      case 'ai':
        return item.is_answered;
      case 'recent':
        // For 'recent', you'd typically sort or filter by date
        // For this example, we'll just show all.
        return true; 
      default:
        return true;
    }
  }).sort((a, b) => {
    // Basic sorting for 'recent' if needed, otherwise relevance is implicit
    if (activeTab === 'recent') {
      return b.creation_date - a.creation_date;
    }
    return b.score - a.score; // Default to sorting by score (relevance)
  });

  const formatNumber = (num: number) => {
    if (num >= 1000000) return `${(num / 1000000).toFixed(1)}M`;
    if (num >= 1000) return `${(num / 1000).toFixed(1)}K`;
    return num.toString();
  };

  const formatDate = (timestamp: number) => {
    const date = new Date(timestamp * 1000);
    const now = new Date();
    const diffInSeconds = (now.getTime() - date.getTime()) / 1000;
    
    if (diffInSeconds < 60) return `${Math.floor(diffInSeconds)}s ago`;
    if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)}m ago`;
    if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)}h ago`;
    if (diffInSeconds < 604800) return `${Math.floor(diffInSeconds / 86400)}d ago`;
    
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  };

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
              <span className={`text-xs px-2 py-0.5 rounded-full 
                ${activeTab === id ? 'bg-blue-600 text-white' : 'bg-gray-200 text-gray-700'}`}>
                {count}
              </span>
            </button>
          ))}
        </div>
      </div>

      {/* Main content area for results */}
      <div className="p-6 max-w-4xl mx-auto">
        <div className="space-y-4">
          {filteredResults.map((item) => (
            <div
              key={item.question_id}
              className="bg-white p-4 border border-gray-200 rounded-md flex hover:border-gray-300 
                       hover:shadow-sm transition-all duration-200 group"
            >
              {/* Vote and Answer status column */}
              <div className="flex flex-col items-center flex-shrink-0 w-20 text-center text-sm mr-4">
                <div className="mb-2">
                  <span className="block text-xl font-semibold text-gray-700">{item.score}</span>
                  <span className="block text-gray-500">votes</span>
                </div>
                <div className={`px-2 py-1 rounded-sm text-xs font-medium 
                  ${item.is_answered 
                    ? 'bg-green-100 text-green-700 border border-green-300' // Answered style
                    : 'bg-gray-100 text-gray-600 border border-gray-300' // Open style
                  }`}>
                  {item.answer_count} {item.is_answered ? 'answers' : 'answer'}
                </div>
                <div className="mt-2 text-gray-500 text-xs">
                  {formatNumber(item.view_count)} views
                </div>
              </div>

              {/* Question details column */}
              <div className="flex-1 min-w-0">
                <h2 className="text-lg font-semibold mb-1 group-hover:text-blue-700 transition-colors">
                  <a 
                    href={item.link} 
                    target="_blank" 
                    rel="noopener noreferrer"
                    className="flex items-start space-x-2 text-blue-800 hover:text-blue-900 hover:underline"
                  >
                    <span className="flex-1 min-w-0 break-words">{item.title}</span>
                    <ExternalLink className="w-4 h-4 opacity-0 group-hover:opacity-100 transition-opacity flex-shrink-0" />
                  </a>
                </h2>

                {/* Tags */}
                <div className="flex flex-wrap gap-2 mb-3">
                  {item.tags.map((tag) => (
                    <span
                      key={tag}
                      className="inline-flex items-center px-2 py-1 bg-blue-50 text-blue-700 
                               text-xs rounded-sm hover:bg-blue-100 transition-colors cursor-pointer"
                    >
                      {tag}
                    </span>
                  ))}
                </div>

                {/* Question metadata (user, date) */}
                <div className="flex justify-end items-center text-xs text-gray-500">
                  {item.owner && (
                    <div className="flex items-center space-x-2 mr-4">
                      {/* Placeholder for owner image if needed */}
                      {/* <img src={item.owner.profile_image} alt={item.owner.display_name} className="w-5 h-5 rounded-full" /> */}
                      <span className="font-medium text-blue-600 hover:text-blue-700 cursor-pointer">
                        {item.owner.display_name}
                      </span>
                      <span className="text-gray-600">({formatNumber(item.owner.reputation)})</span>
                    </div>
                  )}
                  <span>asked {formatDate(item.creation_date)}</span>
                </div>
              </div>
            </div>
          ))}
        </div>

        {results.has_more && (
          <div className="text-center mt-8">
            <button className="px-6 py-3 bg-blue-500 text-white rounded-md hover:bg-blue-600 
                             transition-colors font-semibold shadow-md hover:shadow-lg">
              Load More Results
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default SearchResults;
