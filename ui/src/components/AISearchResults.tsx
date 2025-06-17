import React, { useRef } from 'react';
import { useSearch } from '../hooks/useSearch';
import SearchResultBox from './SearchResultBox';
import useSuggestedAnswer from '../hooks/useLLM';

interface AISearchResultsProps {
    query: string;
}

export const AISearchResults: React.FC<AISearchResultsProps> = ({
    query,
}) => {
    const { searchResults, isLoading, error, aiSort } = useSearch();
    const { answer, loading: answerLoading, error: answerError } = useSuggestedAnswer(query);
    const lastProcessedQuery = useRef<string>('');
  
    React.useEffect(() => {
        if (query && query !== lastProcessedQuery.current) {
            lastProcessedQuery.current = query;
            aiSort(query);
        }
    }, [query, aiSort]);

    return (
        <div className="w-full max-w-[1100px] mx-auto px-6">
            {/* Loading and Error States */}
            {(answerLoading || isLoading) && (
            <div className="p-4 mb-4 bg-[#fdf7e2] text-[#6a737c] border border-[#e6d8be] rounded">
                {answerLoading && <div>Fetching suggested answer...</div>}
                {isLoading && <div>Sorting Answers Using LLM Based on Relevancy...</div>}
            </div>
            )}
            {(error || answerError) && (
            <div className="p-4 mb-4 bg-[#fdf7e2] text-[#c22e32] border border-[#e6d8be] rounded">
                {error && <div>{error}</div>}
                {answerError && <div>Error fetching suggested answer</div>}
            </div>
            )}

            {/* Suggested Answer section */}
            {answer && (
            <div className="mb-6 border border-[#e3e6e8] rounded">
                <div className="bg-[#f8f9f9] p-3 border-b border-[#e3e6e8]">
                    <h3 className="text-[15px] font-medium text-[#232629]">Suggested Answer</h3>
                    <p className="text-xs text-[#6a737c] mt-1">This answer was generated using AI and may not be entirely accurate.</p>
                </div>
                <div className="p-6 text-[#232629]">
                    <div dangerouslySetInnerHTML={{ __html: answer }} />
                </div>
            </div>
            )}

            {/* Search Results */}
            <div className="space-y-3">
            {searchResults?.items?.map((result, idx) => (
                <div key={idx}>
                <SearchResultBox item={result} />
                </div>
            ))}
            </div>
        </div>
    );
};

export default AISearchResults;
