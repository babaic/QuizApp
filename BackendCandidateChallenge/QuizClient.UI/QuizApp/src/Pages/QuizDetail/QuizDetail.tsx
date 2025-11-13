import {useEffect, useState } from "react";
import type {QuizResponseModel} from "../../Models/QuizResponseModel.ts";
import {quiz} from "../../API.ts";
import './QuizDetail.css';
import { useParams } from "react-router";
import QuestionAndAnswersCard from "../../Components/QuestionAndAnswersCard.tsx";

const QuizDetail = () => {
    const {quizId} = useParams<{quizId: string}>();
    const [quizData, setQuizData] = useState<QuizResponseModel | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchQuizDetail = async () => {
            if (!quizId) {
                setError('Quiz ID is missing');
                setLoading(false);
                return;
            }
            try {
                setLoading(true);
                const data = await quiz.getById(quizId);
                setQuizData(data);
                setError(null);
            } catch (err) {
                setError('Failed to load quiz details. Please try again later.');
                console.error('Error fetching quiz details:', err);
            } finally {
                setLoading(false);
            }
        };

        fetchQuizDetail().catch((err) => {
            console.error('Unexpected error in fetchQuizDetail:', err);
            setError('An unexpected error occurred.');
            setLoading(false);
        });
    }, []);

    if (loading) {
        return (
            <div className="quiz-detail-container">
                <div className="loading">Loading quiz...</div>
            </div>
        );
    }

    if (error || !quizData) {
        return (
            <div className="quiz-detail-container">
                <div className="error">{error || 'Quiz not found'}</div>
            </div>
        );
    }

    return (
        <div className="quiz-detail-container">
            <div className="quiz-detail-header">
                <h1 className="quiz-detail-title">{quizData.title}</h1>
                <p className="quiz-detail-info">
                    Quiz ID: #{quizData.id} | Total Questions: {quizData.questions.length || 0}
                </p>
            </div>

            {quizData.questions.length == 0 && (<h3>Empty quiz!</h3>)}
            
            <div className="questions-container">
                {quizData.questions.map((question, index) => (
                    <QuestionAndAnswersCard key={question.id} question={question} index={index}/>
                ))}
            </div>
        </div>
    );
};

export default QuizDetail;