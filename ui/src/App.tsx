import React, { useState } from 'react';
import SearchBar from './components/SearchBar';
import SideNav from './components/SideNav';
import SearchResults from './components/SearchResults';
import { useSearch } from './hooks/useSearch';
import { NavItem } from './types';

const App: React.FC = () => {
  const [activeNavItem, setActiveNavItem] = useState<NavItem>('search');
  const [currentQuery, setCurrentQuery] = useState<string>('');
  const { searchResults, isLoading, error, search, clearResults } = useSearch();

  const handleSearch = async (query: string) => {
    setCurrentQuery(query);
    await search(query);
  };

  const handleNavItemClick = (item: NavItem) => {
    setActiveNavItem(item);
    if (item === 'search') {
      clearResults();
      setCurrentQuery('');
    }
  };

  const renderContent = () => {
    if (error) {
      return (
        <div className="flex-1 flex items-center justify-center min-h-screen bg-gray-50">
          <div className="text-center">
            <div className="text-red-500 text-6xl mb-4">⚠️</div>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Search Error</h2>
            <p className="text-gray-600 mb-4">{error}</p>
            <button 
              onClick={clearResults}
              className="px-6 py-3 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
            >
              Try Again
            </button>
          </div>
        </div>
      );
    }

    if (activeNavItem === 'search' && !searchResults) {
      return <SearchBar onSearch={handleSearch} isLoading={isLoading} />;
    }

    if (searchResults) {
      return (
        <SearchResults 
          results={searchResults} 
          query={currentQuery}
          isLoading={isLoading}
        />
      );
    }

    // Handle other nav items
    return (
      <div className="flex-1 flex items-center justify-center min-h-screen bg-gray-50">
        <div className="text-center">
          <div className="text-6xl mb-4">🚧</div>
          <h2 className="text-2xl font-bold text-gray-900 mb-2">Coming Soon</h2>
          <p className="text-gray-600">
            {activeNavItem === 'recent' && 'Recent questions feature is under development'}
            {activeNavItem === 'popular' && 'Popular questions feature is under development'}
            {activeNavItem === 'unanswered' && 'Unanswered questions feature is under development'}
          </p>
        </div>
      </div>
    );
  };

  return (
    <div className="flex h-screen">
      <SideNav activeItem={activeNavItem} onItemClick={handleNavItemClick} />
      {renderContent()}
    </div>
  );
};

export default App;