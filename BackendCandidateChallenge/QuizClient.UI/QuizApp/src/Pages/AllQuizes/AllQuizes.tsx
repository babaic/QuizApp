import { useEffect, useState } from 'react';
import { quiz } from '../../API.ts';
import type { QuizResponseModel } from '../../Models/QuizResponseModel';
import './AllQuizes.css';
import QuizTitleCard from "../../Components/QuizTitleCard.tsx";

const QuizList = () => {
    const [quizzes, setQuizzes] = useState<QuizResponseModel[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const fetchQuizzes = async () => {
        try {
            setLoading(true);
            const data = await quiz.getAll();
            setQuizzes(data);
            setError(null);
        } catch (err) {
            setError('Failed to load quizzes. Please try again later.');
            console.error('Error fetching quizzes:', err);
        } finally {
            setLoading(false);
        }
    };

    // const openQuizDetails = (quizId: number) => {
    //     navigate(`/quiz/${quizId}`);
    // };

    useEffect(() => {
        void fetchQuizzes();
    }, []);

    if (loading) {
        return (
            <div className="quiz-list-container">
                <div className="loading">Loading quizzes...</div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="quiz-list-container">
                <div className="error">{error}</div>
            </div>
        );
    }

    return (
        <div className="quiz-list-container">
            <h1 className="quiz-list-title">Available Quizzes</h1>
            {quizzes.length === 0 ? (
                <div className="no-quizzes">No quizzes available at the moment.</div>
            ) : (
                <div className="quiz-grid">
                    {quizzes.map((quizItem) => (
                        <QuizTitleCard quizId={quizItem.id} title={quizItem.title} key={quizItem.id} />
                    ))}
                </div>
            )}
        </div>
    );
};

export default QuizList;