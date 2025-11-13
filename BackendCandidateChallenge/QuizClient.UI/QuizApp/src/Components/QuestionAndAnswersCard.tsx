import type {QuestionItem} from "../Models/QuizResponseModel.ts";

interface QuestionAndAnswersCardProps {
    question: QuestionItem;
    index: number;
}

const QuestionAndAnswersCard = ({ question, index }: QuestionAndAnswersCardProps) => {
    return (
        <div className="question-card">
            <div className="question-header">
                <span className="question-number">Question {index + 1}</span>
                {question.score && (
                    <span className="question-score">Score: {question.score}</span>
                )}
            </div>
            <h3 className="question-text">{question.text}</h3>

            <div className="answers-list">
                {question.answers?.map((answer) => (
                    <div
                        key={answer.id}
                        className="answer-option"
                    >
                        <span className="answer-id">#{answer.id}</span>
                        <span className="answer-text">{answer.text}</span>
                    </div>
                ))}
            </div>

            {question.userAnswerId && (
                <div className="user-answer-info">
                    User Answer ID: {question.userAnswerId}
                </div>
            )}

            <div className="correct-answer-info">
                Correct Answer ID: {question.correctAnswerId}
            </div>
        </div>
    );
};

export default QuestionAndAnswersCard;