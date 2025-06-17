import React from 'react';
import { ExternalLink } from 'lucide-react';
import { SearchResult } from '../types';

interface SearchResultBoxProps {
    item: SearchResult;
}

const formatNumber = (num: number): string => {
    return new Intl.NumberFormat('en-US').format(num);
};

const formatDate = (timestamp: number): string => {
    return new Date(timestamp * 1000).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
};

const SearchResultBox: React.FC<SearchResultBoxProps> = ({ item }) => {
    return (
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
                <div
                    className={`px-2 py-1 rounded-sm text-xs font-medium 
                        ${item.is_answered 
                            ? 'bg-green-100 text-green-700 border border-green-300'
                            : 'bg-gray-100 text-gray-600 border border-gray-300'
                        }`}
                >
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
    );
};

export default SearchResultBox;