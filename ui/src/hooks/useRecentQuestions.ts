
import { useEffect, useState } from "react";

function useRecentQuestions() {
  // Define state with string-typed properties
  const [questions, setQuestions] = useState(
    [] as { title: string; timestamp: string }[]
  );
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null as string | null);

  useEffect(() => {
    async function fetchQuestions() {
      setLoading(true);
      setError(null);
      try {
        const response = await fetch("http://localhost:5196/api/Question/recent", {
          headers: { Accept: "text/plain" },
        });

        if (!response.ok) {
          throw new Error(`Failed with status ${response.status}`);
        }

        // The API might respond with application/json
        const data = (await response.json()) as { title: string; timestamp: string }[];

        // We’re forcing everything to be string-typed
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
