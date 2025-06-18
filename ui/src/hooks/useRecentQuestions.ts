interface Question {
  title: string;
  timestamp: string;
}

import { useEffect, useState } from "react";

function useRecentQuestions() {
  const [questions, setQuestions] = useState<Question[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchQuestions() {
      setLoading(true);
      setError(null);
      try {
        const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/Question/recent`, {
          headers: { Accept: "text/plain" },
        });

        if (!response.ok) {
          throw new Error(`Failed with status ${response.status}`);
        }

        const data = await response.json() as Question[];

        // We're forcing everything to be string-typed
        const parsed = data.map((item) => ({
          title: String(item.title),
          timestamp: String(item.timestamp),
        }));

        setQuestions(parsed);
      } catch (err) {
        setError((err as Error).message);
      } finally {
        setLoading(false);
      }
    }

    fetchQuestions();
  }, []);

  return { questions, loading, error };
}

export default useRecentQuestions;
