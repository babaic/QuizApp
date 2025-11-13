import type {QuizResponseModel} from "./Models/QuizResponseModel.ts";

const API_BASE_URL = 'http://localhost:5001/api';

const quiz = {
    getAll: async (): Promise<QuizResponseModel[]> => {
        const response = await fetch(`${API_BASE_URL}/quizzes/`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
        });
        if(!response.ok) throw new Error(response.statusText);
        
        return await response.json();
    },
    getById: async (id: string): Promise<QuizResponseModel> => {
        const response = await fetch(`${API_BASE_URL}/quizzes/${id}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
        });
        if(!response.ok) throw new Error(response.statusText);
        
        return await response.json();
    }
}

export { quiz };