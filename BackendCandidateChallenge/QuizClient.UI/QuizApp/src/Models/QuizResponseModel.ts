export interface AnswerItem {
    id: number;
    text: string;
}

export interface QuestionItem {
    id: number;
    text: string;
    answers: AnswerItem[];
    userAnswerId?: number | null;
    correctAnswerId: number;
    score?: number | null;
}

export interface QuizResponseModel {
    id: number;
    title: string;
    questions: QuestionItem[];
    links: Record<string, string>;
}