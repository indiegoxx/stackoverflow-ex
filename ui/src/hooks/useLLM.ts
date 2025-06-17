import { useState, useEffect } from "react";

function useSuggestedAnswer(question: string) {
  const [answer, setAnswer] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    console.log(import.meta.env.VITE_API_BASE_URL);
    if (!question) return;

    setLoading(true);
    setError(null);

    fetch(
      `${import.meta.env.VITE_API_BASE_URL}/api/Question/suggested-answer?question=${encodeURIComponent(
        question
      )}`,
      { headers: { accept: "text/plain" } }
    )
      .then((response) => {
        if (!response.ok) {
          throw new Error("Failed to fetch suggested answer.");
        }
        return response.json();
      })
      .then((data) => {
        setAnswer(data.answer);
        setLoading(false);
      })
      .catch((err) => {
        setError(err);
        setLoading(false);
      });
  }, [question]);

  return { answer, loading, error };
}

export default useSuggestedAnswer;
