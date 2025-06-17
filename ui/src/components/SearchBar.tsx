import React, { useState, useRef, useEffect } from 'react';
import { Search, Keyboard } from 'lucide-react';

interface SearchBarProps {
  onSearch: (query: string) => void;
  isLoading?: boolean;
}

const SearchBar: React.FC<SearchBarProps> = ({ onSearch, isLoading = false }) => {
  const [query, setQuery] = useState('');
  const [isFocused, setIsFocused] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault();
        inputRef.current?.focus();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, []);

  const handleSearch = () => {
    if (query.trim()) {
      onSearch(query.trim());
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  return (
    <div className="flex justify-center w-full items-center min-h-screen bg-[#f8f9f9] px-4 py-8">
      <div className="w-full max-w-2xl">
        <div className="text-center mb-6">
          <div className="flex items-center justify-center mb-4">
            <div className="p-3 bg-[#f48225] rounded-lg">
              <Search className="w-6 h-6 text-white" />
            </div>
          </div>
          <h1 className="text-3xl font-bold text-[#232629] mb-2">
            Find Your Answer
          </h1>
          <p className="text-[#6a737c] text-base">
            Search through questions on stack overflow
          </p>
        </div>

        <div className="relative">
          <div className={`
            relative bg-white rounded-md transition-all duration-300 border
            ${isFocused ? 'border-[#379fef] shadow-md' : 'border-[#babfc4] hover:border-[#838c95]'}
          `}>
            <div className="flex items-center p-3">
              <Search className={`w-5 h-5 mr-3 ${isFocused ? 'text-[#379fef]' : 'text-[#838c95]'}`} />
              <input
                ref={inputRef}
                type="text"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                onFocus={() => setIsFocused(true)}
                onBlur={() => setIsFocused(false)}
                onKeyPress={handleKeyPress}
                placeholder="Ask a question or search for answers..."
                className="flex-1 text-base outline-none placeholder-[#838c95]"
                disabled={isLoading}
              />
              <div className="flex items-center space-x-3">
                <div className="hidden sm:flex items-center space-x-1 text-xs text-[#6a737c] bg-[#f1f2f3] px-2 py-1 rounded">
                  <Keyboard className="w-3 h-3" />
                  <span>Ctrl+K</span>
                </div>
                <button
                  onClick={handleSearch}
                  disabled={isLoading || !query.trim()}
                  className="px-4 py-2 bg-[#0a95ff] text-white rounded hover:bg-[#0074cc] 
                           disabled:opacity-50 disabled:cursor-not-allowed transition-all
                           text-sm font-medium"
                >
                  {isLoading ? (
                    <div className="flex items-center space-x-2">
                      <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                      <span>Searching...</span>
                    </div>
                  ) : (
                    'Search'
                  )}
                </button>
              </div>
            </div>
          </div>

          <div className="mt-4 flex flex-wrap justify-center gap-2">
            {['React', 'JavaScript', 'Python', 'TypeScript', 'CSS'].map((tag) => (
              <button
                key={tag}
                onClick={() => {
                  setQuery(tag);
                  onSearch(tag);
                }}
                className="px-3 py-1.5 bg-[#e1ecf4] text-[#39739d] rounded-md hover:bg-[#d0e3f1] 
                         transition-colors text-sm"
              >
                {tag}
              </button>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default SearchBar;