export interface SearchResult {
  question_id: number;
  title: string;
  link: string;
  score: number;
  answer_count: number;
  view_count: number;
  tags: string[];
  is_answered: boolean;
  creation_date: number;
  owner: {
    display_name: string;
    reputation: number;
  };
  relevanceScore: string;
}

export interface SearchResponse {
  items: SearchResult[];
  has_more: boolean;
  quota_max: number;
  quota_remaining: number;
  total?: number;
}

export type NavItem = 'search' | 'recent' | 'popular' | 'unanswered';
export type TabType = 'general' | 'ai' | 'recent';