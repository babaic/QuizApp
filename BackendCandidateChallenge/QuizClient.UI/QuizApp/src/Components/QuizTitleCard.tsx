import { useNavigate } from "react-router";

interface QuizTitleCardProps {
    quizId: number;
    title: string;
}

const QuizTitleCard = ({quizId, title}: QuizTitleCardProps) => {
    const navigate = useNavigate();
    const openQuizDetails = (quizId: number) => {
        navigate(`/quiz/${quizId}`);
    };
    
    return (
        <div key={quizId} className="quiz-card">
            <div className="quiz-card-header">
                <h2 className="quiz-title">{title}</h2>
            </div>
            <div className="quiz-card-footer">
                <button onClick={() => openQuizDetails(quizId)} className="start-quiz-btn">View Quiz</button>
            </div>
        </div>
    )
}

export default QuizTitleCard;