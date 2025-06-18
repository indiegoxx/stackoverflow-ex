import { useState } from "react";
import useRecentQuestions from "../hooks/useRecentQuestions";

interface RecentProps {
    onQuestionSelect?: (question: any) => void; // Add proper type instead of 'any' based on your question type
}

export default function Recent({ onQuestionSelect }: RecentProps) {
        const { questions, loading, error } = useRecentQuestions();
        const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');

        if (loading) {
                return <div className="flex justify-center p-4">Loading...</div>;
        }

        if (error) {
                return <div className="text-red-600 p-4">Error: {error}</div>;
        }

        const sortedQuestions = [...questions].sort((a, b) => {
                const comparison = new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime();
                return sortOrder === 'asc' ? comparison : -comparison;
        });

        return (
                <div className="w-full mx-auto p-4">
                        <div className="flex justify-between items-center mb-4">
                                <h2 className="text-2xl font-bold text-gray-900">Recent Questions</h2>
                                <button
                                        onClick={() => setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')}
                                        className="px-3 py-1 text-sm bg-gray-100 rounded hover:bg-gray-200"
                                >
                                        Sort {sortOrder === 'asc' ? '↑' : '↓'}
                                </button>
                        </div>
                        
                        {questions.length === 0 ? (
                                <div className="text-center py-8">
                                        <p className="text-gray-500">No questions have been asked yet.</p>
                                        <p className="text-sm text-gray-400 mt-2">Be the first one to ask a question!</p>
                                </div>
                        ) : (
                                <ul className="divide-y divide-gray-200">
                                        {sortedQuestions.map((question, index) => (
                                                <li 
                                                        key={index} 
                                                        className="py-4 hover:bg-gray-50 cursor-pointer"
                                                        onClick={() => onQuestionSelect?.(question)}
                                                >
                                                        <h3 className="text-lg font-medium text-blue-600 hover:text-blue-800 mb-2">
                                                                {question.title}
                                                        </h3>
                                                        <time 
                                                                dateTime={question.timestamp}
                                                                className="text-sm text-gray-500"
                                                        >
                                                                {new Date(question.timestamp).toLocaleString()}
                                                        </time>
                                                </li>
                                        ))}
                                </ul>
                        )}
                </div>
        );
}